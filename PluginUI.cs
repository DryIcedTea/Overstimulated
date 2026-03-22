using UnityEngine;

namespace OvercookedBP
{
    
    //I for the life of me cannot get checkbox buttons to work with IL2CPP, so I'm doing keyboard toggling with a basic ass UI for those without console enabled.
    public class PluginUI : MonoBehaviour
    {
        private bool _active = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                _active = !_active;
                Plugin.Log.LogInfo($"[Filter] Player filter menu {(_active ? "opened" : "closed")}. Current state:");
                LogCurrentState();
            }
            
            if (Input.GetKeyDown(KeyCode.P))
            {
                _ = Plugin.BP.StopDevices();
            }

            if (!_active) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                PlayerFilter.TogglePlayer(PlayerInputLookup.Player.One);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                PlayerFilter.TogglePlayer(PlayerInputLookup.Player.Two);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                PlayerFilter.TogglePlayer(PlayerInputLookup.Player.Three);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                PlayerFilter.TogglePlayer(PlayerInputLookup.Player.Four);
        }

        private void LogCurrentState()
        {
            Plugin.Log.LogInfo($"  Player One:   {(PlayerFilter.EnabledPlayers.Contains(PlayerInputLookup.Player.One) ? "ON" : "OFF")} (press 1 to toggle)");
            Plugin.Log.LogInfo($"  Player Two:   {(PlayerFilter.EnabledPlayers.Contains(PlayerInputLookup.Player.Two) ? "ON" : "OFF")} (press 2 to toggle)");
            Plugin.Log.LogInfo($"  Player Three: {(PlayerFilter.EnabledPlayers.Contains(PlayerInputLookup.Player.Three) ? "ON" : "OFF")} (press 3 to toggle)");
            Plugin.Log.LogInfo($"  Player Four:  {(PlayerFilter.EnabledPlayers.Contains(PlayerInputLookup.Player.Four) ? "ON" : "OFF")} (press 4 to toggle)");
        }
        
        private void OnGUI()
        {
            if (!_active) return;

            float x = (Screen.width - 300) / 2f;
            float y = (Screen.height - 180) / 2f;

            GUI.Box(new Rect(x, y, 300, 180), "OvercookedBP - Player Filter");

            GUI.Label(new Rect(x + 10, y + 30, 280, 20), $"Player One:   {(PlayerFilter.EnabledPlayers.Contains(PlayerInputLookup.Player.One) ? "ON" : "OFF")}");
            GUI.Label(new Rect(x + 10, y + 55, 280, 20), $"Player Two:   {(PlayerFilter.EnabledPlayers.Contains(PlayerInputLookup.Player.Two) ? "ON" : "OFF")}");
            GUI.Label(new Rect(x + 10, y + 80, 280, 20), $"Player Three: {(PlayerFilter.EnabledPlayers.Contains(PlayerInputLookup.Player.Three) ? "ON" : "OFF")}");
            GUI.Label(new Rect(x + 10, y + 105, 280, 20), $"Player Four:  {(PlayerFilter.EnabledPlayers.Contains(PlayerInputLookup.Player.Four) ? "ON" : "OFF")}");

            GUI.Label(new Rect(x + 10, y + 135, 280, 20), "Press 1-4 to toggle | INSERT to close");
        }
    }

    public static class PlayerFilter
    {
        
        public static System.Collections.Generic.HashSet<PlayerInputLookup.Player> EnabledPlayers = new System.Collections.Generic.HashSet<PlayerInputLookup.Player>
        {
            PlayerInputLookup.Player.One
        };

        public static bool IsEnabled(PlayerInputLookup.Player? player)
        {
            if (player == null) return false;
            return EnabledPlayers.Contains(player.Value);
        }

        public static void TogglePlayer(PlayerInputLookup.Player player)
        {
            if (EnabledPlayers.Contains(player))
                EnabledPlayers.Remove(player);
            else
                EnabledPlayers.Add(player);

            Plugin.Log.LogInfo($"[Filter] Player {player}: {(EnabledPlayers.Contains(player) ? "ON" : "OFF")}");
            Plugin.Log.LogInfo($"[Filter] Active players: {string.Join(", ", EnabledPlayers)}");
        }
    }
}