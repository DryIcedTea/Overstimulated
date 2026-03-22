using System;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using OrderController;
using Team17.Online.Multiplayer.Messaging;
using UnityEngine;

namespace OvercookedBP;

[BepInPlugin("DryIcedMatcha.OvercookedBP", "OvercookedBP", "0.1.0")]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;
    internal static BPManager BP;
    internal static PluginConfig Cfg;

    private Harmony _harmony;

    public override void Load()
    {
        Log = base.Log;
        Cfg = new PluginConfig(Config);

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        BP = new BPManager(Log);
        AddComponent<PluginUI>();
        var connectionTask = Task.Run(async () =>
        {
            try
            {
                await BP.ConnectButtplug(Cfg.IntifaceIP.Value);
                Log.LogInfo("About to scan...");
                await BP.ScanForDevices(5000);
                Log.LogInfo("Scan done.");
                await BP.VibrateDevicesPulse(50, 1000);
            }
            catch (Exception ex)
            {
                Log.LogError($"Top level error: {ex}");
            }
        });

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll();
        Log.LogInfo("Harmony patches applied successfully!");
    }
}

// ==========================================
// CHOPPING
// ==========================================

[HarmonyPatch(typeof(ClientWorkstation), nameof(ClientWorkstation.StartWorking))]
public class ClientWorkstation_StartWorking_Patch
{
    public static void Postfix(ClientWorkstation __instance, GameObject _interacter, ClientWorkableItem _item)
    {
        if (!Plugin.Cfg.ChoppingEnabled.Value) return;
        var playerID = _interacter?.GetComponent<PlayerIDProvider>();
        if (!PlayerFilter.IsEnabled(playerID?.GetID())) return;
        Plugin.Log.LogInfo($"[Workstation] StartWorking triggered by Player: {playerID?.GetID()} IsLocal: {playerID?.IsLocallyControlled()}");
        _ = Plugin.BP.VibrateDevices(Plugin.Cfg.ChoppingIntensity.Value);
    }
}

[HarmonyPatch(typeof(ClientWorkstation), nameof(ClientWorkstation.StopWorking))]
public class ClientWorkstation_StopWorking_Patch
{
    public static void Postfix(ClientWorkstation __instance, GameObject _interacter)
    {
        if (!Plugin.Cfg.ChoppingEnabled.Value) return;
        var playerID = _interacter?.GetComponent<PlayerIDProvider>();
        if (!PlayerFilter.IsEnabled(playerID?.GetID())) return;
        Plugin.Log.LogInfo($"[Workstation] StopWorking triggered by Player: {playerID?.GetID()} IsLocal: {playerID?.IsLocallyControlled()}");
        _ = Plugin.BP.StopDevices();
    }
}

// ==========================================
// ORDER SYSTEM
// ==========================================

[HarmonyPatch(typeof(ClientOrderControllerBase), nameof(ClientOrderControllerBase.AddNewOrder))]
public class ClientOrderControllerBase_AddNewOrder_Patch
{
    public static void Postfix(ClientOrderControllerBase __instance, Serialisable _data)
    {
        if (!Plugin.Cfg.OrdersEnabled.Value) return;
        Plugin.Log.LogInfo("[Orders] New order added!");
        _ = Plugin.BP.VibrateDevicesPulse(Plugin.Cfg.OrderNewIntensity.Value, 200);
    }
}

[HarmonyPatch(typeof(ClientOrderControllerBase), nameof(ClientOrderControllerBase.OnFoodDelivered))]
public class ClientOrderControllerBase_OnFoodDelivered_Patch
{
    public static void Postfix(ClientOrderControllerBase __instance, bool _success, OrderID _orderID)
    {
        if (!Plugin.Cfg.OrdersEnabled.Value) return;

        if (_success)
        {
            Plugin.Log.LogInfo("[Orders] Order delivered successfully!");
            _ = Plugin.BP.VibrateDevicesPulse(Plugin.Cfg.OrderDeliveredIntensity.Value, 500);
        }
        else
        {
            Plugin.Log.LogInfo("[Orders] Order failed!");
            _ = Plugin.BP.VibrateDevicesPulse(Plugin.Cfg.OrderFailedOrExpiredIntensity.Value, 400);
        }
    }
}

[HarmonyPatch(typeof(ClientOrderControllerBase), nameof(ClientOrderControllerBase.OnOrderExpired))]
public class ClientOrderControllerBase_OnOrderExpired_Patch
{
    public static void Postfix(ClientOrderControllerBase __instance, OrderID _orderID)
    {
        if (!Plugin.Cfg.OrdersEnabled.Value) return;
        Plugin.Log.LogInfo("[Orders] Order expired!");
        _ = Plugin.BP.VibrateDevicesPulse(Plugin.Cfg.OrderFailedOrExpiredIntensity.Value, 400);
    }
}

// ==========================================
// FIRE EXTINGUISHER
// ==========================================

[HarmonyPatch(typeof(ClientSprayingUtensil), nameof(ClientSprayingUtensil.StartSpray))]
public class ClientSprayingUtensil_StartSpray_Patch
{
    public static void Postfix(ClientSprayingUtensil __instance)
    {
        if (!Plugin.Cfg.ExtinguisherEnabled.Value) return;
        var playerID = __instance.m_carrier?.GetComponent<PlayerIDProvider>();
        if (!PlayerFilter.IsEnabled(playerID?.GetID())) return;
        Plugin.Log.LogInfo($"[FireExtinguisher] StartSpray by Player: {playerID?.GetID()} IsLocal: {playerID?.IsLocallyControlled()}");
        _ = Plugin.BP.VibrateDevices(Plugin.Cfg.ExtinguisherIntensity.Value);
    }
}

[HarmonyPatch(typeof(ClientSprayingUtensil), nameof(ClientSprayingUtensil.StopSpray))]
public class ClientSprayingUtensil_StopSpray_Patch
{
    public static void Postfix(ClientSprayingUtensil __instance)
    {
        if (!Plugin.Cfg.ExtinguisherEnabled.Value) return;
        var playerID = __instance.m_carrier?.GetComponent<PlayerIDProvider>();
        if (!PlayerFilter.IsEnabled(playerID?.GetID())) return;
        Plugin.Log.LogInfo($"[FireExtinguisher] StopSpray by Player: {playerID?.GetID()} IsLocal: {playerID?.IsLocallyControlled()}");
        _ = Plugin.BP.StopDevices();
    }
}

// ==========================================
// PLAYER DEATH / RESPAWN
// ==========================================
[HarmonyPatch(typeof(ClientPlayerRespawnBehaviour), nameof(ClientPlayerRespawnBehaviour.UpdateSynchronisingX))]
public class ClientPlayerRespawnBehaviour_StateTracking_Patch
{
    private static System.Collections.Generic.Dictionary<IntPtr, bool> playerRespawnStates = new();

    public static void Postfix(ClientPlayerRespawnBehaviour __instance)
    {
        if (!Plugin.Cfg.RespawnEnabled.Value) return;

        IntPtr ptr = __instance.Pointer;
        bool isCurrentlyRespawning = __instance.m_isRespawning;

        if (!playerRespawnStates.ContainsKey(ptr))
        {
            playerRespawnStates[ptr] = isCurrentlyRespawning;
            return;
        }

        bool wasRespawning = playerRespawnStates[ptr];

        if (isCurrentlyRespawning && !wasRespawning)
        {
            var playerID = __instance.GetComponent<PlayerIDProvider>();
            if (!PlayerFilter.IsEnabled(playerID?.GetID())) return;
            Plugin.Log.LogInfo($"[Death] BeginRespawn by Player: {playerID?.GetID()} IsLocal: {playerID?.IsLocallyControlled()}");
            _ = Plugin.BP.VibrateDevices(Plugin.Cfg.DeathIntensity.Value);
        }
        else if (!isCurrentlyRespawning && wasRespawning)
        {
            var playerID = __instance.GetComponent<PlayerIDProvider>();
            if (!PlayerFilter.IsEnabled(playerID?.GetID())) return;
            Plugin.Log.LogInfo($"[Death] Respawned Player: {playerID?.GetID()} IsLocal: {playerID?.IsLocallyControlled()}");
            _ = Plugin.BP.StopDevices();
        }

        playerRespawnStates[ptr] = isCurrentlyRespawning;
    }
}
// ==========================================
// DASHING
// ==========================================

[HarmonyPatch(typeof(ClientPlayerControlsImpl_Default), nameof(ClientPlayerControlsImpl_Default.DoDash))]
public class ClientPlayerControlsImpl_Default_DoDash_Patch
{

    public static void Postfix(ClientPlayerControlsImpl_Default __instance)
    {
        if (!Plugin.Cfg.DashEnabled.Value) return;
        
        var playerID = __instance.m_playerIDProvider;
        if (!PlayerFilter.IsEnabled(playerID?.GetID())) return;
        Plugin.Log.LogInfo($"[Dash] Player: {playerID?.GetID()} IsLocal: {playerID?.IsLocallyControlled()}");
        _ = Plugin.BP.VibrateDevicesPulse(Plugin.Cfg.DashIntensity.Value, 200);
    }
}



// ==========================================
// WASHING - The sink itself didn't have a thing for tracking what player used it, so I ended up with this instead.
// ==========================================
[HarmonyPatch(typeof(ClientPlayerControlsImpl_Default), "Update_Impl")]
public class ClientPlayerControlsImpl_Default_WashingTracker_Patch
{
    private static System.Collections.Generic.Dictionary<IntPtr, bool> _playerWashingStates = new();

    public static void Postfix(ClientPlayerControlsImpl_Default __instance)
    {
        if (__instance.m_playerIDProvider == null) return;
        if (!Plugin.Cfg.WashingEnabled.Value) return;

        IntPtr ptr = __instance.Pointer;
        bool isCurrentlyWashing = false;

        if (__instance.m_lastInteracted != null)
        {
            ClientWashingStation sink = __instance.m_lastInteracted.GetComponent<ClientWashingStation>();
            if (sink != null && sink.m_isWashing)
                isCurrentlyWashing = true;
        }

        if (!_playerWashingStates.ContainsKey(ptr))
        {
            _playerWashingStates[ptr] = isCurrentlyWashing;
            return;
        }

        bool wasWashing = _playerWashingStates[ptr];

        if (isCurrentlyWashing && !wasWashing)
        {
            var playerID = __instance.m_playerIDProvider;
            if (!PlayerFilter.IsEnabled(playerID?.GetID())) return;
            
            Plugin.Log.LogInfo($"[Washing] Started washing by Player: {playerID?.GetID()} IsLocal: {playerID?.IsLocallyControlled()}");
            if (PlayerFilter.IsEnabled(playerID?.GetID()))
                _ = Plugin.BP.VibrateDevices(Plugin.Cfg.WashingIntensity.Value);
        }
        else if (!isCurrentlyWashing && wasWashing)
        {
            var playerID = __instance.m_playerIDProvider;
            if (!PlayerFilter.IsEnabled(playerID?.GetID())) return;
            
            Plugin.Log.LogInfo($"[Washing] Stopped washing by Player: {playerID?.GetID()} IsLocal: {playerID?.IsLocallyControlled()}");
            if (PlayerFilter.IsEnabled(playerID?.GetID()))
                _ = Plugin.BP.StopDevices();
        }

        _playerWashingStates[ptr] = isCurrentlyWashing;
    }
}