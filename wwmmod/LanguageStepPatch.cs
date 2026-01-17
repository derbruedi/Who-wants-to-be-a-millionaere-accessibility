using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

// ==========================================================
// PATCH: Initial Language Selection Screen (First Launch)
// This is the FIRST screen users see - critical for accessibility!
// ==========================================================

public static class LanguageStepState
{
    public static string LastSpokenLanguage = "";
    public static bool WasMoving = false;
}

// Patch 1: Read language when selection changes (ShowHideOrange is called after movement)
[HarmonyPatch(typeof(LanguageStep), "ShowHideOrange")]
public static class LanguageStepShowHideOrangePatch
{
    static void Postfix(LanguageStep __instance, bool _show)
    {
        // Only read when showing the highlight (not hiding)
        if (!_show) return;

        try
        {
            // Find the currently selected country (the one with orange highlight enabled)
            var countriesField = AccessTools.Field(typeof(LanguageStep), "mCountries");
            var countries = (Array)countriesField.GetValue(__instance);

            if (countries == null) return;

            for (int i = 0; i < countries.Length; i++)
            {
                object countryInfo = countries.GetValue(i);
                var countryType = countryInfo.GetType();

                // Get the orange highlight Image
                var orangeField = countryType.GetField("mOrange");
                Image orangeImage = (Image)orangeField.GetValue(countryInfo);

                if (orangeImage != null && orangeImage.enabled)
                {
                    // Get the text label
                    var txtField = countryType.GetField("mTxt");
                    Text txtComponent = (Text)txtField.GetValue(countryInfo);

                    // Get the country enum for additional context
                    var countryEnumField = countryType.GetField("mCountry");
                    object countryEnum = countryEnumField.GetValue(countryInfo);

                    string languageName = txtComponent != null ? txtComponent.text : countryEnum.ToString();

                    // Only speak if changed
                    if (languageName != LanguageStepState.LastSpokenLanguage)
                    {
                        LanguageStepState.LastSpokenLanguage = languageName;

                        string ttsMessage = languageName;
                        MelonLogger.Msg($"[TTS-Language] Selected: {ttsMessage}");
                        TolkHelper.Speak(ttsMessage);
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-Language] Error in ShowHideOrange patch: {ex.Message}");
        }
    }
}

// Patch 2: Announce when the language selection screen first appears
[HarmonyPatch(typeof(LanguageStep), "StartStep")]
public static class LanguageStepStartPatch
{
    static void Postfix(LanguageStep __instance)
    {
        try
        {
            // Reset state
            LanguageStepState.LastSpokenLanguage = "";

            // Small delay message to let user know the screen is ready
            string welcomeMessage = "Language selection. Use left and right arrows to choose your language, then press Enter to confirm.";
            MelonLogger.Msg($"[TTS-Language] Screen opened");
            TolkHelper.Speak(welcomeMessage);

            // The ShowHideOrange patch will announce the initially selected language
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-Language] Error in StartStep patch: {ex.Message}");
        }
    }
}

// Patch 3: Announce when language is confirmed
[HarmonyPatch(typeof(LanguageStep), "FinishStep")]
public static class LanguageStepFinishPatch
{
    static void Postfix(LanguageStep __instance)
    {
        try
        {
            string message = $"{LanguageStepState.LastSpokenLanguage} selected.";
            MelonLogger.Msg($"[TTS-Language] Confirmed: {message}");
            TolkHelper.Speak(message);

            // Reset state
            LanguageStepState.LastSpokenLanguage = "";
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-Language] Error in FinishStep patch: {ex.Message}");
        }
    }
}
