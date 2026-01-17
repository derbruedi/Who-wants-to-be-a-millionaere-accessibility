using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

// ==========================================================
// HILFSKLASSE: Speichert den letzten angesagten Index
// ==========================================================
public static class MenuFocusState
{
    public static int LastMenuIndex = -1;
}

// ==========================================================
// PATCH 3: Hauptmenü-Navigation (Tastatur/Controller)
// Ziel: MainMenu.MoveInMenu()
// ==========================================================
[HarmonyPatch(typeof(MainMenu), "MoveInMenu")]
public static class MainMenuNavigationPatch
{
    static void Postfix(MainMenu __instance)
    {
        // 1. Hole den aktuell fokussierten Index (mFirstHighlighted)
        int newIndex = (int)AccessTools.Field(typeof(MainMenu), "mFirstHighlighted").GetValue(__instance);
        
        // 2. Nur ausführen, wenn sich der Fokus geändert hat
        if (newIndex != -1 && newIndex != MenuFocusState.LastMenuIndex)
        {
            try
            {
                // Hole das Array der Menü-Container (mMainMenuContainers)
                MainMenu.MainMenuContainer[] containers = (MainMenu.MainMenuContainer[])AccessTools.Field(typeof(MainMenu), "mMainMenuContainers").GetValue(__instance);

                if (containers != null && containers.Length > newIndex && containers[newIndex].mtxt != null)
                {
                    // HIER KOMMT DER TEXT VOM SPIEL:
                    string buttonText = containers[newIndex].mtxt.text;
                    MainMenu.eMainMenuValue menuValue = containers[newIndex].mValue;

                    // --- ÄNDERUNG: Nur noch der reine Spieltext ---
                    string ttsMessage = buttonText; 
                    
                    // Loggen (neutral) und Sprechen
                    MelonLogger.Msg($"[TTS] {menuValue}: {ttsMessage}");
                    TolkHelper.Speak(ttsMessage); 

                    MenuFocusState.LastMenuIndex = newIndex;
                }
            }
            catch (System.Exception ex)
            {
                // Fehlermeldung auf Englisch/Neutral geändert
                MelonLogger.Error($"Error accessing menu focus: {ex.Message}");
            }
        }
    }
}