using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;

// ==========================================================
// PATCH: Publikumsjoker (Taste P)
// ==========================================================
[HarmonyPatch(typeof(UIController), "Update")]
public static class PublicJokerPatch
{
    static void Postfix(UIController __instance)
    {
        // Listen for 'P' key (for Public/Audience)
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpeakAudienceResults(__instance);
        }
    }

    static void SpeakAudienceResults(UIController ui)
    {
        try
        {
            // Sicherheitscheck: Ist das Array da?
            // In deinem Code heißt es: askPublicContainers
            if (ui.askPublicContainers == null || ui.askPublicContainers.Length == 0)
            {
                TolkHelper.Speak("Audience results not found.");
                return;
            }

            string fullText = "Audience vote: ";
            string[] answerLetters = { "A", "B", "C", "D" };
            bool foundAny = false;

            // Wir gehen durch alle 4 Container (A, B, C, D)
            for (int i = 0; i < ui.askPublicContainers.Length; i++)
            {
                // Zugriff auf das Struct-Element
                // Achtung: Bei Struct-Arrays in C# muss man manchmal aufpassen, 
                // aber da wir nur lesen, sollte der direkte Zugriff auf .percent funktionieren.
                
                Text percentText = ui.askPublicContainers[i].percent;

                if (percentText != null && !string.IsNullOrEmpty(percentText.text))
                {
                    // Der Text im Spiel ist z.B. "45%"
                    // Wir bauen den Satz: "A: 45%, "
                    string letter = (i < 4) ? answerLetters[i] : "?";
                    fullText += $"{letter}: {percentText.text}. ";
                    foundAny = true;
                }
            }

            if (foundAny)
            {
                MelonLogger.Msg($"[TTS-Public] {fullText}");
                TolkHelper.Speak(fullText);
            }
            else
            {
                // Falls die Balken noch nicht da sind (man hat den Joker noch nicht gedrückt)
                TolkHelper.Speak("No audience results visible.");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Fehler im PublicJokerPatch: {ex.Message}");
            TolkHelper.Speak("Error reading audience results.");
        }
    }
}