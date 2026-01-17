using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

// HINWEIS: OptionFocusState wird von OptionMenuCompletePatch bereitgestellt.

// ==========================================================
// HILFSKLASSE: Typ-Auflösung für GameplayMenuContainer
// ==========================================================
public static class GameplayReflector
{
    // Methode zum Abrufen des Typs des Containers
    public static Type GetGameplayMenuContainerType()
    {
        // Wir verwenden nur den Klassennamen, da er als public struct definiert ist
        return AccessTools.TypeByName("GameplayMenuContainer"); 
    }
}

// ==========================================================
// PATCH 10: Navigation in Auswahl-Menüs (Modus, Schwierigkeit etc.)
// Ziel: SimpleChoice.MoveInMenuSimple()
// ==========================================================
[HarmonyPatch(typeof(SimpleChoice), "MoveInMenuSimple")] 
public static class SimpleChoiceNavigationPatch
{
    static void Postfix(SimpleChoice __instance)
    {
        // Wenn sich der Menüpunkt durch die Maus geändert hat, wird mCurrentSelected in IsInMenuSimple() aktualisiert.
        // Wenn sich der Menüpunkt durch die Tastatur geändert hat, wurde mCurrentSelected in MoveInMenuSimple() aktualisiert.
        // Wir prüfen, ob sich der neue Wert (mCurrentSelected) von dem alten Wert (mOldSelected) unterscheidet.
        
        int newIndex = (int)AccessTools.Field(typeof(SimpleChoice), "mCurrentSelected").GetValue(__instance);
        int oldIndex = (int)AccessTools.Field(typeof(SimpleChoice), "mOldSelected").GetValue(__instance);

        // Prüfen, ob sich der Fokus tatsächlich geändert hat und ob wir uns nicht gerade in der Maus-Prüfung befinden (old != new)
        if (newIndex != oldIndex)
        {
            try
            {
                Type containerType = GameplayReflector.GetGameplayMenuContainerType();
                System.Array containers = (System.Array)AccessTools.Field(typeof(SimpleChoice), "mGameplayChoices").GetValue(__instance);

                if (containers != null && containers.Length > newIndex)
                {
                    object currentContainer = containers.GetValue(newIndex);
                    
                    // Auslesen des Menütextes (mText)
                    Text menuTextComponent = (Text)AccessTools.Field(containerType, "mText").GetValue(currentContainer);
                    
                    string menuText = menuTextComponent?.text ?? "Menu Item (Text not found)";
                    
                    // Auslesen des Datenwertes (mDataValue) für Kontext
                    object dataValue = AccessTools.Field(containerType, "mDataValue").GetValue(currentContainer);

                    string ttsMessage = $"{menuText} selected.";
                    
                    MelonLogger.Msg($"[TTS-SimpleChoice] Focus: {ttsMessage} (Value: {dataValue})");
                    TolkHelper.Speak(ttsMessage);
                    
                    // WICHTIG: Setze mOldSelected nicht direkt hier, da das Spiel das selbst macht.
                    // Aber wir haben die Ansage ausgelöst.
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error reading SimpleChoice Focus: {ex.Message}");
            }
        }
    }
}