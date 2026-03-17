using Buttplug.Client;
using Buttplug.Core;
using Buttplug.Core.Messages;
using System;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Logging;
using UnityEngine;

namespace OvercookedBP
{
    internal class BPManager
    {
        private ButtplugClient _client;
        private ManualLogSource _logger;
        private DateTime _lastVibrateTime = DateTime.MinValue;

        public BPManager(ManualLogSource logger)
        {
            _logger = logger;
        }

        public async Task ConnectButtplug(string intifaceIP)
        {
            if (_client?.Connected == true)
            {
                _logger.LogInfo("Buttplug already connected, skipping.");
                return;
            }

            _client = new ButtplugClient("OvercookedBP");

            _client.DeviceAdded += (_, args) => _logger.LogInfo($"[+] Device connected: {args.Device.Name}");
            _client.DeviceRemoved += (_, args) => _logger.LogInfo($"[-] Device disconnected: {args.Device.Name}");
            _client.ServerDisconnect += (_, _) => _logger.LogInfo("[!] Intiface server disconnected.");
            _client.ErrorReceived += (_, args) => _logger.LogError($"[!] Error: {args.Exception.Message}");

            _logger.LogInfo($"Connecting to ws://{intifaceIP}...");

            try
            {
                await _client.ConnectAsync($"ws://{intifaceIP}");
            }
            catch (ButtplugClientConnectorException)
            {
                _logger.LogError("Could not connect to Intiface Central! Make sure it is running and the server is started.");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected connection error: {ex.Message}");
                return;
            }

            _logger.LogInfo($"Connected! {_client.Devices.Length} device(s) already available:");
            foreach (var device in _client.Devices)
                _logger.LogInfo($"  - {device.Name}");
        }

        public async Task DisconnectButtplug()
        {
            if (_client?.Connected != true)
            {
                _logger.LogInfo("Buttplug not connected, skipping.");
                return;
            }

            _logger.LogInfo("Stopping all devices and disconnecting...");
            await StopDevices();
            await _client.DisconnectAsync();
        }

        public async Task ScanForDevices(int durationMs = 5000)
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

        public async Task VibrateDevices(float level)
        {
            if (!HasVibrators()) return;

            float intensity = Mathf.Clamp(level, 0f, 100f) / 100f;

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

        public async Task VibrateDevicesPulse(float level, int durationMs = 400)
        {
            if (!HasVibrators()) return;

            _lastVibrateTime = DateTime.Now;
            var myTime = _lastVibrateTime;

            await VibrateDevices(level);

            _ = Task.Run(async () =>
            {
                await Task.Delay(durationMs);
                if (_lastVibrateTime == myTime)
                    await StopDevices();
            });
        }

        public async Task StopDevices()
        {
            if (_client?.Connected != true) return;

            _logger.LogInfo("Stopping all devices.");
            foreach (var device in _client.Devices)
            {
                if (!device.HasOutput(OutputType.Vibrate)) continue;
                await device.RunOutputAsync(DeviceOutput.Vibrate.Percent(1));
                await Task.Delay(20);
                await device.RunOutputAsync(DeviceOutput.Vibrate.Percent(0));
                await Task.Delay(20);
                await device.RunOutputAsync(DeviceOutput.Vibrate.Percent(0));
            }
            
        }

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