using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

// ==========================================================
// PATCH: Online Menü & Lobby
// ==========================================================

// 1. Navigation in der Punkteliste (Scoreboard)
[HarmonyPatch(typeof(UIController), "OnlineMoveSelectedPlayer")]
public static class OnlineScoreNavPatch
{
    static void Postfix(UIController __instance)
    {
        try
        {
            // Holt das aktuell markierte Spieler-Objekt
            object onlinePlayer = __instance.GetCurrentHilightedPlayer();
            
            if (onlinePlayer != null)
            {
                // Wir nutzen Reflection, um an die Daten im 'OnlinePlayer'-Objekt zu kommen
                // (da wir die Klasse OnlinePlayer nicht direkt im Code haben)
                Type playerType = onlinePlayer.GetType();
                
                string name = (string)AccessTools.Field(playerType, "mDisplayedName").GetValue(onlinePlayer);
                object score = AccessTools.Field(playerType, "mScore").GetValue(onlinePlayer); // Score ist meist int
                
                string tts = $"{name}. Score: {score}";
                
                MelonLogger.Msg($"[TTS-Online] {tts}");
                TolkHelper.Speak(tts);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Fehler beim Lesen des Online-Scores: {ex.Message}");
        }
    }
}

// 2. Automatische Ansage, wenn sich die Spielerzahl in der Lobby ändert
[HarmonyPatch(typeof(UIController), "OnlineUpdateLobbyPlayerCountText")]
public static class OnlineLobbyCountPatch
{
    static void Postfix(int value)
    {
        // Sagt z.B.: "Players: 4"
        TolkHelper.Speak($"Players joined: {value}");
    }
}

// 3. Manuelle Status-Abfrage in der Lobby (Taste O)
[HarmonyPatch(typeof(UIController), "Update")]
public static class OnlineLobbyStatusPatch
{
    static void Postfix(UIController __instance)
    {
        if (Input.GetKeyDown(KeyCode.O)) // O for Online
        {
            ReadLobbyStatus(__instance);
        }
    }

    static void ReadLobbyStatus(UIController ui)
    {
        try
        {
            // Prüfen, ob wir überhaupt im Online-Modus sind
            if (!ui.mIsOnlineMode) return;

            string info = "";

            // Spieleranzahl lesen
            if (ui.mOnlineLobbyPlayerCountText != null && ui.mOnlineLobbyPlayerCountText.gameObject.activeInHierarchy)
            {
                info += $"Players: {ui.mOnlineLobbyPlayerCountText.text}. ";
            }

            // Timer lesen (Wie lange noch warten?)
            if (ui.mOnlineLobbyTimerText != null && ui.mOnlineLobbyTimerText.gameObject.activeInHierarchy)
            {
                info += $"Time remaining: {ui.mOnlineLobbyTimerText.text}";
            }

            if (!string.IsNullOrEmpty(info))
            {
                TolkHelper.Speak(info);
            }
            else
            {
                TolkHelper.Speak("No lobby information available.");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Fehler bei Lobby-Status (O): {ex.Message}");
        }
    }
}