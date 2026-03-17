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

    private Harmony _harmony;

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        
        BP = new BPManager(Log);
        var connectionTask = Task.Run(async () =>
        {
            try
            {
                await BP.ConnectButtplug("127.0.0.1:12345");
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
        Plugin.Log.LogInfo($"[Workstation] StartWorking {DateTime.Now:HH:mm:ss.fff}");
        _ = Plugin.BP.VibrateDevices(50);
    }
}

[HarmonyPatch(typeof(ClientWorkstation), nameof(ClientWorkstation.StopWorking))]
public class ClientWorkstation_StopWorking_Patch
{
    public static void Postfix(ClientWorkstation __instance, GameObject _interacter)
    {
        Plugin.Log.LogInfo($"[Workstation] StopWorking {DateTime.Now:HH:mm:ss.fff}");
        _ = Plugin.BP.StopDevices();
    }
}

[HarmonyPatch(typeof(ClientCookingStation), nameof(ClientCookingStation.SetCookerOn))]
public class ClientCookingStation_SetCookerOn_Patch
{
    public static void Postfix(ClientCookingStation __instance, bool _isOn)
    {
        Plugin.Log.LogInfo($"[CookingStation] SetCookerOn triggered. isOn = {_isOn}");
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
        Plugin.Log.LogInfo("[Orders] New order added!");
        _ = Plugin.BP.VibrateDevicesPulse(30, 200);
        
    }
}

[HarmonyPatch(typeof(ClientOrderControllerBase), nameof(ClientOrderControllerBase.OnFoodDelivered))]
public class ClientOrderControllerBase_OnFoodDelivered_Patch
{
    public static void Postfix(ClientOrderControllerBase __instance, bool _success, OrderID _orderID)
    {
        if (_success)
        {
            Plugin.Log.LogInfo("[Orders] Order delivered successfully!");   
            _ = Plugin.BP.VibrateDevicesPulse(100, 500);
        }

        else
        {
            Plugin.Log.LogInfo("[Orders] Order failed!");
            _ = Plugin.BP.VibrateDevicesPulse(50, 400);
        }
            
    }
}

[HarmonyPatch(typeof(ClientOrderControllerBase), nameof(ClientOrderControllerBase.OnOrderExpired))]
public class ClientOrderControllerBase_OnOrderExpired_Patch
{
    public static void Postfix(ClientOrderControllerBase __instance, OrderID _orderID)
    {
        Plugin.Log.LogInfo("[Orders] Order expired!");
        _ = Plugin.BP.VibrateDevicesPulse(50, 400);
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
        bool isWashing = __instance.m_isWashing;

        if (isWashing && !_wasWashing)
        {
            Plugin.Log.LogInfo("[Washing] Started washing!");
            _ = Plugin.BP.VibrateDevices(50);
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
        Plugin.Log.LogInfo("[FireExtinguisher] Started spraying!");
        _ = Plugin.BP.VibrateDevices(60);
    }
}

[HarmonyPatch(typeof(ClientSprayingUtensil), nameof(ClientSprayingUtensil.StopSpray))]
public class ClientSprayingUtensil_StopSpray_Patch
{
    public static void Postfix(ClientSprayingUtensil __instance)
    {
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
    //Track respawn state of each player
    private static System.Collections.Generic.Dictionary<IntPtr, bool> playerRespawnStates = new System.Collections.Generic.Dictionary<IntPtr, bool>();

    public static void Postfix(ClientPlayerRespawnBehaviour __instance)
    {
        IntPtr ptr = __instance.Pointer;
        bool isCurrentlyRespawning = __instance.m_isRespawning;

        //1st time seein player
        if (!playerRespawnStates.ContainsKey(ptr))
        {
            playerRespawnStates[ptr] = isCurrentlyRespawning;
            return;
        }

        bool wasRespawning = playerRespawnStates[ptr];
        
        if (isCurrentlyRespawning && !wasRespawning)
        {
            Plugin.Log.LogInfo("[Respawn] Player died!");
            
             _ = Plugin.BP.VibrateDevices(75);
        }
        else if (!isCurrentlyRespawning && wasRespawning)
        {
            Plugin.Log.LogInfo("[Respawn] Player respawned!");
            
             _ = Plugin.BP.StopDevices();
        }
        
        playerRespawnStates[ptr] = isCurrentlyRespawning;
    }
}