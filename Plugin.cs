using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace OvercookedBP;

[BepInPlugin("DryIcedMatcha.OvercookedBP", "OvercookedBP", "0.1.0")]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;
    private Harmony _harmony;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        // Initialize and apply all Harmony patches in this assembly
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll();
        
        Log.LogInfo("Harmony patches applied successfully!");
    }
}

// ==========================================
// CHOPPING (ClientWorkstation)
// ==========================================

[HarmonyPatch(typeof(ClientWorkstation), nameof(ClientWorkstation.OnChop))]
public class ClientWorkstation_OnChop_Patch
{
    public static void Postfix(ClientWorkstation __instance, Transform _interacter)
    {
        Plugin.Log.LogInfo("[Workstation] OnChop Triggered");
    }
}

[HarmonyPatch(typeof(ClientWorkstation), nameof(ClientWorkstation.StartWorking))]
public class ClientWorkstation_StartWorking_Patch
{
    public static void Postfix(ClientWorkstation __instance, GameObject _interacter, ClientWorkableItem _item)
    {
        Plugin.Log.LogInfo("[Workstation] StartWorking triggered");
    }
}

[HarmonyPatch(typeof(ClientWorkstation), nameof(ClientWorkstation.StopWorking))]
public class ClientWorkstation_StopWorking_Patch
{
    public static void Postfix(ClientWorkstation __instance, GameObject _interacter)
    {
        Plugin.Log.LogInfo("[Workstation] StopWorking triggered");
    }
}

// ==========================================
// COOKING (ClientCookingStation - Stoves/Pots)
// ==========================================

[HarmonyPatch(typeof(ClientCookingStation), nameof(ClientCookingStation.SetCooking))]
public class ClientCookingStation_SetCooking_Patch
{
    public static void Postfix(ClientCookingStation __instance, bool _isCooking)
    {
        Plugin.Log.LogInfo($"[CookingStation] SetCooking triggered. isCooking = {_isCooking}");
        
        if (_isCooking) 
        {
            Plugin.Log.LogInfo("   -> POT IS COOKING");
        }
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
// MIXING (ClientMixingStation - Blenders)
// ==========================================

[HarmonyPatch(typeof(ClientMixingStation), nameof(ClientMixingStation.OnItemAdded))]
public class ClientMixingStation_OnItemAdded_Patch
{
    public static void Postfix(ClientMixingStation __instance)
    {
        Plugin.Log.LogInfo("[MixingStation] Item Added to blender!");
        
        bool isOn = __instance.m_isTurnedOn;
        Plugin.Log.LogInfo($"   -> Blender Status: {isOn}");
    }
}