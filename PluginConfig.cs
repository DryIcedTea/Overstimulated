using BepInEx.Configuration;

namespace OvercookedBP;

public class PluginConfig
{
    // ==========================================
    // General
    // ==========================================
    public ConfigEntry<string> IntifaceIP { get; private set; }

    // ==========================================
    // Chopping (Workstation)
    // ==========================================
    public ConfigEntry<bool>  ChoppingEnabled   { get; private set; }
    public ConfigEntry<int>   ChoppingIntensity  { get; private set; }

    // ==========================================
    // Order System
    // ==========================================
    public ConfigEntry<bool>  OrdersEnabled              { get; private set; }
    public ConfigEntry<int>   OrderNewIntensity           { get; private set; }
    public ConfigEntry<int>   OrderDeliveredIntensity     { get; private set; }
    public ConfigEntry<int>   OrderFailedOrExpiredIntensity { get; private set; }

    // ==========================================
    // Washing
    // ==========================================
    public ConfigEntry<bool>  WashingEnabled    { get; private set; }
    public ConfigEntry<int>   WashingIntensity   { get; private set; }

    // ==========================================
    // Fire Extinguisher
    // ==========================================
    public ConfigEntry<bool>  ExtinguisherEnabled   { get; private set; }
    public ConfigEntry<int>   ExtinguisherIntensity  { get; private set; }

    // ==========================================
    // Player Death / Respawn
    // ==========================================
    public ConfigEntry<bool>  RespawnEnabled    { get; private set; }
    public ConfigEntry<int>   DeathIntensity     { get; private set; }
    
    // ==========================================
    // Dashing
    // ==========================================
    
    public ConfigEntry<bool>  DashEnabled    { get; private set; }
    public ConfigEntry<int>   DashIntensity     { get; private set; }

    public PluginConfig(ConfigFile cfg)
    {
        // == General ============================================================
        IntifaceIP = cfg.Bind(
            "General",
            "IntifaceIP",
            "127.0.0.1:12345",
            "IP address and port of the Intiface Central server.");

        // == Chopping ============================================================
        ChoppingEnabled = cfg.Bind(
            "Chopping",
            "Enabled",
            true,
            "Enable vibrations while chopping at a workstation.");

        ChoppingIntensity = cfg.Bind(
            "Chopping",
            "Intensity",
            50,
            new ConfigDescription(
                "Vibration intensity while chopping (0–100).",
                new AcceptableValueRange<int>(0, 100)));

        // == Orders ============================================================
        OrdersEnabled = cfg.Bind(
            "Orders",
            "Enabled",
            true,
            "Enable vibrations for all order events (new, delivered, failed/expired).");

        OrderNewIntensity = cfg.Bind(
            "Orders",
            "NewOrderIntensity",
            30,
            new ConfigDescription(
                "Vibration intensity when a new order arrives (0–100).",
                new AcceptableValueRange<int>(0, 100)));

        OrderDeliveredIntensity = cfg.Bind(
            "Orders",
            "DeliveredIntensity",
            100,
            new ConfigDescription(
                "Vibration intensity when an order is delivered successfully (0–100).",
                new AcceptableValueRange<int>(0, 100)));

        OrderFailedOrExpiredIntensity = cfg.Bind(
            "Orders",
            "FailedOrExpiredIntensity",
            50,
            new ConfigDescription(
                "Vibration intensity when an order fails or expires (0–100).",
                new AcceptableValueRange<int>(0, 100)));

        // == Washing ============================================================
        WashingEnabled = cfg.Bind(
            "Washing",
            "Enabled",
            true,
            "Enable vibrations while washing dishes. (THIS WORKS FOR EVERY PLAYER REGARDLESS OF SETTING)");

        WashingIntensity = cfg.Bind(
            "Washing",
            "Intensity",
            50,
            new ConfigDescription(
                "Vibration intensity while washing (0–100).",
                new AcceptableValueRange<int>(0, 100)));

        // == Fire Extinguisher ============================================================
        ExtinguisherEnabled = cfg.Bind(
            "FireExtinguisher",
            "Enabled",
            true,
            "Enable vibrations while using the fire extinguisher.");

        ExtinguisherIntensity = cfg.Bind(
            "FireExtinguisher",
            "Intensity",
            60,
            new ConfigDescription(
                "Vibration intensity while spraying (0–100).",
                new AcceptableValueRange<int>(0, 100)));

        // == Respawn ============================================================
        RespawnEnabled = cfg.Bind(
            "Respawn",
            "Enabled",
            true,
            "Enable vibrations on player death.");

        DeathIntensity = cfg.Bind(
            "Respawn",
            "DeathIntensity",
            75,
            new ConfigDescription(
                "Vibration intensity when a player dies (0–100).",
                new AcceptableValueRange<int>(0, 100)));
        
        // == Dashing ============================================================
        DashEnabled = cfg.Bind(
            "Dashing",
            "Enabled",
            true,
            "Enable vibrations on player death.");

        DashIntensity = cfg.Bind(
            "Dashing",
            "DashIntensity",
            30,
            new ConfigDescription(
                "Vibration intensity when dashing (0–100).",
                new AcceptableValueRange<int>(0, 100)));
    }
}