using Buttplug.Client;
using Buttplug.Core;
using Buttplug.Core.Messages;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using UnityEngine;

namespace OvercookedBP
{
    internal class BPManager
    {
        private ButtplugClient _client;
        private string _intifaceIP;
        private ManualLogSource _logger;

        private CancellationTokenSource _currentVibrationCts;
        private readonly object _vibrationLock = new object();

        public BPManager(ManualLogSource logger)
        {
            _logger = logger;
        }

        public async Task ConnectButtplug(string intifaceIP)
        {
            _intifaceIP = intifaceIP;

            if (_client?.Connected == true)
            {
                _logger.LogInfo("Buttplug already connected, skipping.");
                return;
            }
            
            if (_client != null)
            {
                _client.DeviceAdded -= HandleDeviceAdded;
                _client.DeviceRemoved -= HandleDeviceRemoved;
                _client.Dispose();
            }

            _client = new ButtplugClient("OvercookedBP");

            
            _client.DeviceAdded += HandleDeviceAdded;
            _client.DeviceRemoved += HandleDeviceRemoved;
            _client.ServerDisconnect += (_, _) => _logger.LogInfo("Intiface server disconnected.");
            _client.ErrorReceived += (_, args) => _logger.LogError($"Buttplug async error: {args.Exception.Message}");

            _logger.LogInfo($"Connecting to ws://{_intifaceIP}...");

            try
            {
                await _client.ConnectAsync($"ws://{_intifaceIP}");
            }
            catch (ButtplugClientConnectorException ex)
            {
                _logger.LogError($"Could not connect to Intiface Central (is it running?): {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected connection error: {ex.Message}");
                return;
            }

            _logger.LogInfo($"Connected! {_client.Devices.Length} device(s) already available:");
            foreach (var device in _client.Devices)
                _logger.LogInfo($"  - {device.Name} (Index: {device.Index})");
        }

        public async Task DisconnectButtplug()
        {
            if (_client?.Connected != true)
            {
                _logger.LogInfo("Buttplug not connected, skipping.");
                return;
            }

            _logger.LogInfo("Stopping all devices and disconnecting...");
            await _client.StopAllDevicesAsync();
            await _client.DisconnectAsync();
        }

        public async Task ScanForDevices(int durationMs = 30000)
        {
            if (_client?.Connected != true)
            {
                _logger.LogInfo("Buttplug not connected, cannot scan.");
                return;
            }

            _logger.LogInfo($"Scanning for devices for {durationMs / 1000}s...");
            await _client.StartScanningAsync();
            await Task.Delay(durationMs);
            await _client.StopScanningAsync();
            _logger.LogInfo("Scan complete.");
        }

        /// <summary>
        /// Vibrates all connected vibrating devices continuously at the given level (0–100).
        /// </summary>
        public async Task VibrateDevice(float level)
        {
            if (!HasVibrators()) return;

            float intensity = Mathf.Clamp(level, 0f, 100f) / 100f;

            if (intensity <= 0f)
            {
                await StopDevices();
                return;
            }

            foreach (var device in _client.Devices)
            {
                if (!device.HasOutput(OutputType.Vibrate)) continue;

                try
                {
                    _logger.LogInfo($"Vibrating {device.Name} at {level}%");
                    await device.RunOutputAsync(DeviceOutput.Vibrate.Percent(intensity));
                }
                catch (ButtplugDeviceException ex)
                {
                    _logger.LogError($"Failed to vibrate {device.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Vibrates at the given level (0–100) for a short pulse (default 400ms), then stops.
        /// </summary>
        public Task VibrateDevicePulse(float level) => VibrateDevicePulse(level, 400);

        public async Task VibrateDevicePulse(float level, int durationMs)
        {
            if (!HasVibrators()) return;
            
            CancellationToken token;
            lock (_vibrationLock)
            {
                _currentVibrationCts?.Cancel();
                _currentVibrationCts = new CancellationTokenSource();
                token = _currentVibrationCts.Token;
            }

            _logger.LogInfo($"VibrateDevicePulse {Mathf.Clamp(level, 0f, 100f)}% for {durationMs}ms");

            try
            {
                await VibrateDevice(level);
                await Task.Delay(durationMs, token);

                
                if (!token.IsCancellationRequested)
                    await StopDevices();
            }
            catch (TaskCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during vibration pulse: {ex.Message}");
                await StopDevices();
            }
        }

        public async Task StopDevices()
        {
            if (_client?.Connected != true) return;

            _logger.LogInfo("Stopping all devices.");
            await _client.StopAllDevicesAsync();
        }

        private void HandleDeviceAdded(object sender, DeviceAddedEventArgs e) =>
            _logger.LogInfo($"[+] Device connected: {e.Device.Name} (Index: {e.Device.Index})");

        private void HandleDeviceRemoved(object sender, DeviceRemovedEventArgs e) =>
            _logger.LogInfo($"[-] Device disconnected: {e.Device.Name}");

        private bool HasVibrators()
        {
            if (_client?.Connected != true)
            {
                _logger.LogInfo("Buttplug not connected.");
                return false;
            }

            if (!_client.Devices.Any(d => d.HasOutput(OutputType.Vibrate)))
            {
                _logger.LogInfo("No connected devices have vibrators.");
                return false;
            }

            return true;
        }
    }
}