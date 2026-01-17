using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;

// ==========================================================
// PATCH: Geldstatus abfragen 
// Taste M = Um wie viel spielen wir gerade?
// Taste W = Wie viel habe ich sicher (wenn ich aufhöre)?
// ==========================================================
[HarmonyPatch(typeof(UIController), "Update")]
public static class MoneyStatusPatch
{
    static void Postfix(UIController __instance)
    {
        // Key 'M' for "Money" (Current question value)
        if (Input.GetKeyDown(KeyCode.M))
        {
            SpeakCurrentTarget(__instance);
        }

        // Taste 'W' für "Won" (Bereits erspielt / Mitnahme-Betrag)
        if (Input.GetKeyDown(KeyCode.W))
        {
            SpeakAlreadyWon(__instance);
        }
    }

    // Sagt an, um was wir gerade spielen (Aktuelle Stufe)
    static void SpeakCurrentTarget(UIController ui)
    {
        try
        {
            if (ui.mSteps == null || ui.mSteps.Length == 0) return;

            int index = ui.mPyramidIndex;

            if (index >= 0 && index < ui.mSteps.Length)
            {
                Text valueText = ui.mSteps[index].mValue;
                if (valueText != null)
                {
                    // Ansage: "Playing for: 500"
                    string amount = valueText.text;
                    TolkHelper.Speak($"Playing for: {amount}");
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error in MoneyPatch (Current): {ex.Message}");
        }
    }

    // Sagt an, was wir schon haben (Vorherige Stufe)
    static void SpeakAlreadyWon(UIController ui)
    {
        try
        {
            if (ui.mSteps == null || ui.mSteps.Length == 0) return;

            // Wir wollen die Stufe VOR der aktuellen
            int index = ui.mPyramidIndex - 1;

            // Wenn Index kleiner 0 ist, sind wir bei Frage 1 und haben noch nichts gewonnen.
            if (index < 0)
            {
                TolkHelper.Speak("Current winnings: 0");
                return;
            }

            if (index < ui.mSteps.Length)
            {
                Text valueText = ui.mSteps[index].mValue;
                if (valueText != null)
                {
                    // Ansage: "Won: 500"
                    string amount = valueText.text;
                    TolkHelper.Speak($"Won: {amount}");
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error in MoneyPatch (Won): {ex.Message}");
        }
    }
}