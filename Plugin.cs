using System;
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
// CHOPPING (ClientWorkstation)
// ==========================================

[HarmonyPatch(typeof(ClientWorkstation), nameof(ClientWorkstation.StartWorking))]
public class ClientWorkstation_StartWorking_Patch
{
    public static void Postfix(ClientWorkstation __instance, GameObject _interacter, ClientWorkableItem _item)
    {
        if (!Plugin.Cfg.ChoppingEnabled.Value) return;
        Plugin.Log.LogInfo($"[Workstation] StartWorking {DateTime.Now:HH:mm:ss.fff}");
        _ = Plugin.BP.VibrateDevices(Plugin.Cfg.ChoppingIntensity.Value);
    }
}

[HarmonyPatch(typeof(ClientWorkstation), nameof(ClientWorkstation.StopWorking))]
public class ClientWorkstation_StopWorking_Patch
{
    public static void Postfix(ClientWorkstation __instance, GameObject _interacter)
    {
        if (!Plugin.Cfg.ChoppingEnabled.Value) return;
        Plugin.Log.LogInfo($"[Workstation] StopWorking {DateTime.Now:HH:mm:ss.fff}");
        _ = Plugin.BP.StopDevices();
    }
}

// ==========================================
// ORDER SYSTEM (ClientOrderControllerBase)
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
// WASHING (ClientWashingStation)
// ==========================================

[HarmonyPatch(typeof(ClientWashingStation), nameof(ClientWashingStation.UpdateSynchronisingX))]
public class ClientWashingStation_UpdateSynchronisingX_Patch
{
    private static bool _wasWashing = false;

    public static void Postfix(ClientWashingStation __instance)
    {
        if (!Plugin.Cfg.WashingEnabled.Value) return;

        bool isWashing = __instance.m_isWashing;

        if (isWashing && !_wasWashing)
        {
            Plugin.Log.LogInfo("[Washing] Started washing!");
            _ = Plugin.BP.VibrateDevices(Plugin.Cfg.WashingIntensity.Value);
        }
        else if (!isWashing && _wasWashing)
        {
            Plugin.Log.LogInfo("[Washing] Stopped washing!");
            _ = Plugin.BP.StopDevices();
        }

        _wasWashing = isWashing;
    }
}

// ==========================================
// FIRE EXTINGUISHER (ClientSprayingUtensil)
// ==========================================

[HarmonyPatch(typeof(ClientSprayingUtensil), nameof(ClientSprayingUtensil.StartSpray))]
public class ClientSprayingUtensil_StartSpray_Patch
{
    public static void Postfix(ClientSprayingUtensil __instance)
    {
        if (!Plugin.Cfg.ExtinguisherEnabled.Value) return;
        Plugin.Log.LogInfo("[FireExtinguisher] Started spraying!");
        _ = Plugin.BP.VibrateDevices(Plugin.Cfg.ExtinguisherIntensity.Value);
    }
}

[HarmonyPatch(typeof(ClientSprayingUtensil), nameof(ClientSprayingUtensil.StopSpray))]
public class ClientSprayingUtensil_StopSpray_Patch
{
    public static void Postfix(ClientSprayingUtensil __instance)
    {
        if (!Plugin.Cfg.ExtinguisherEnabled.Value) return;
        Plugin.Log.LogInfo("[FireExtinguisher] Stopped spraying!");
        _ = Plugin.BP.StopDevices();
    }
}

// ==========================================
// PLAYER DEATH / RESPAWN (State Tracking)
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
            Plugin.Log.LogInfo("[Respawn] Player died!");
            _ = Plugin.BP.VibrateDevices(Plugin.Cfg.DeathIntensity.Value);
        }
        else if (!isCurrentlyRespawning && wasRespawning)
        {
            Plugin.Log.LogInfo("[Respawn] Player respawned!");
            _ = Plugin.BP.StopDevices();
        }

        playerRespawnStates[ptr] = isCurrentlyRespawning;
    }
}