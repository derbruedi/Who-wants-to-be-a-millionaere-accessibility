using HarmonyLib;
using MelonLoader;
using UnityEngine;
using System;

// ==========================================================
// PATCH: Charakter-Auswahl
// Liest den Namen vor, wenn man im Menü den Kandidaten wechselt.
// ==========================================================
[HarmonyPatch(typeof(CharacterManager), "ShowCurrentPlayer")]
public static class CharacterSelectionPatch
{
    // Wir merken uns den letzten Namen hier direkt in der Klasse
    private static string _lastSpokenCharacter = "";

    static void Postfix(CharacterManager __instance)
    {
        try
        {
            // 1. Welcher Charakter ist gerade aktiv?
            // Die Methode GetCurrentPlayer() liefert z.B. JULIE, HENG etc.
            var characterEnum = __instance.GetCurrentPlayer();

            // 2. Name in Text umwandeln
            string rawName = characterEnum.ToString();

            // 3. Unterstriche durch Leerzeichen ersetzen (z.B. ANNE_MARIE -> ANNE MARIE)
            string niceName = rawName.Replace("_", " ");

            // 4. Nur sprechen, wenn es ein NEUER Charakter ist
            // (ShowCurrentPlayer wird vom Spiel manchmal oft hintereinander aufgerufen)
            if (niceName != _lastSpokenCharacter)
            {
                string ttsMessage = $"Character: {niceName}";
                
                MelonLogger.Msg($"[TTS-Char] {ttsMessage}");
                TolkHelper.Speak(ttsMessage);

                // Merken für das nächste Mal
                _lastSpokenCharacter = niceName;
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Fehler im Charakter-Patch: {ex.Message}");
        }
    }
}