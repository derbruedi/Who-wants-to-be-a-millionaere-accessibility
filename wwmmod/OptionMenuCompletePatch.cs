using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Reflection;

// ==========================================================
// HILFSKLASSE: Speichert den letzten Index für die Fokusprüfung
// ==========================================================
public static class OptionFocusState
{
    public static int LastOptionIndex = -1;
    public static int LastCategoryValue = -1; 
}

// ==========================================================
// HILFSKLASSE: ENTHÄLT DIE REFLECTION-METHODEN (KORREKTUR)
// ==========================================================
public static class OptionReflector
{
    // Typ-Auflösung für OptionAudioContainer
    public static Type GetOptionAudioContainerType()
    {
        return AccessTools.TypeByName("OptionAudioContainer") ?? AccessTools.TypeByName("OptionAudio+OptionAudioContainer");
    }

    // Typ-Auflösung für OptionVideoContainer
    public static Type GetOptionVideoContainerType()
    {
        return AccessTools.TypeByName("OptionVideoContainer") ?? AccessTools.TypeByName("OptionVideo+OptionVideoContainer");
    }
}


// ==========================================================
// PATCH 1: KATEGORIE-WECHSEL (MENUOPTION.CURRENTINIT)
// ==========================================================
[HarmonyPatch(typeof(MenuOption), "CurrentInit", new Type[0])] 
public static class MenuCategoryNavigationPatch
{
    static void Postfix(MenuOption __instance)
    {
        try
        {
            object currentMenuObj = AccessTools.Field(typeof(MenuOption), "mCurrentMenu").GetValue(__instance);
            string categoryName = currentMenuObj.ToString();
            int categoryValue = (int)currentMenuObj; 
            
            if (categoryValue != OptionFocusState.LastCategoryValue)
            {
                string ttsMessage = $"{categoryName} menu selected.";
                MelonLogger.Msg($"[TTS-Options] Category Change: {ttsMessage}");
                TolkHelper.Speak(ttsMessage);
                OptionFocusState.LastCategoryValue = categoryValue;
            }
        }
        catch (Exception ex) { MelonLogger.Error($"Error reading Option Category: {ex.Message}"); }
    }
}

// ==========================================================
// AUDIO-MENÜ PATCHES (VERTICALE UND WERTÄNDERUNG)
// ==========================================================
[HarmonyPatch(typeof(OptionAudio), "MoveInMenuSimple")] 
public static class OptionAudioNavigationPatch
{
    static void Postfix(OptionAudio __instance)
    {
        int newIndex = (int)AccessTools.Field(typeof(OptionAudio), "mCurrentSelected").GetValue(__instance);
        if (newIndex != -1 && newIndex != OptionFocusState.LastOptionIndex)
        {
            try
            {
                // ZUGRIFF ÜBER HILFSKLASSE
                Type containerType = OptionReflector.GetOptionAudioContainerType(); 
                System.Array containers = (System.Array)AccessTools.Field(typeof(OptionAudio), "mAudioChoices").GetValue(__instance);
                if (containers != null && containers.Length > newIndex)
                {
                    object currentContainer = containers.GetValue(newIndex);
                    
                    object dataValue = AccessTools.Field(containerType, "mDataValue").GetValue(currentContainer);
                    Slider sliderComponent = (Slider)AccessTools.Field(containerType, "mSlider").GetValue(currentContainer);
                    
                    string optionName = dataValue.ToString();
                    int currentValue = (sliderComponent != null) ? (int)Math.Round(sliderComponent.value) : 0;
                    
                    string ttsMessage = $"{optionName}. Current value: {currentValue} percent."; 
                    
                    MelonLogger.Msg($"[TTS-OptionAudio] Focus: {ttsMessage}");
                    TolkHelper.Speak(ttsMessage);
                    OptionFocusState.LastOptionIndex = newIndex;
                }
            }
            catch (Exception ex) { MelonLogger.Error($"Error reading Option Focus (Audio): {ex.Message}"); }
        }
    }
}

[HarmonyPatch(typeof(OptionAudio), "AddRemove")] 
public static class OptionAudioSliderPatch
{
    static void Postfix(OptionAudio __instance)
    {
        try
        {
            // ZUGRIFF ÜBER HILFSKLASSE
            Type containerType = OptionReflector.GetOptionAudioContainerType(); 
            int currentSelected = (int)AccessTools.Field(typeof(OptionAudio), "mCurrentSelected").GetValue(__instance);
            System.Array containers = (System.Array)AccessTools.Field(typeof(OptionAudio), "mAudioChoices").GetValue(__instance);
            object currentContainer = containers.GetValue(currentSelected);
            
            object dataValue = AccessTools.Field(containerType, "mDataValue").GetValue(currentContainer);
            Slider sliderComponent = (Slider)AccessTools.Field(containerType, "mSlider").GetValue(currentContainer);

            string optionName = dataValue.ToString();
            int currentValue = (int)Math.Round(sliderComponent.value);
            string ttsMessage = $"{optionName}. Changed to {currentValue} percent.";
            
            MelonLogger.Msg($"[TTS-OptionAudio] Value Change: {ttsMessage}");
            TolkHelper.Speak(ttsMessage);
        }
        catch (Exception ex) { MelonLogger.Error($"Error reading Slider value (Audio): {ex.Message}"); }
    }
}

// ==========================================================
// SPRACHE-MENÜ PATCHES (Unverändert, funktioniert)
// ==========================================================
[HarmonyPatch(typeof(OptionLanguage), "MoveInMenuSimple")] 
public static class OptionLanguageNavigationPatch
{
    static void Postfix(OptionLanguage __instance)
    {
        int newIndex = (int)AccessTools.Field(typeof(OptionLanguage), "mCurrentSelected").GetValue(__instance);
        if (newIndex != -1 && newIndex != OptionFocusState.LastOptionIndex)
        {
            try
            {
                System.Array containers = (System.Array)AccessTools.Field(typeof(OptionLanguage), "mLanguageChoice").GetValue(__instance);
                if (containers != null && containers.Length > newIndex)
                {
                    object currentContainer = containers.GetValue(newIndex);
                    object dataValue = AccessTools.Field(typeof(OptionLanguageContainer), "mDataValue").GetValue(currentContainer);
                    string optionKey = dataValue.ToString();
                    string optionName = optionKey;
                    string currentValue = "Unknown";
                    
                    if (optionKey == "SUBTITLES")
                    {
                        Text subtitleTextComponent = (Text)AccessTools.Field(typeof(OptionLanguage), "mSubtitle").GetValue(__instance);
                        currentValue = subtitleTextComponent.text;
                    }
                    else if (optionKey == "COUNTRY")
                    {
                        Text countryTextComponent = (Text)AccessTools.Field(typeof(OptionLanguage), "mCountryTxt").GetValue(__instance);
                        currentValue = countryTextComponent.text;
                    }
                    
                    string ttsMessage = $"{optionName}. Current value: {currentValue}.";
                    MelonLogger.Msg($"[TTS-OptionLanguage] Focus: {ttsMessage}");
                    TolkHelper.Speak(ttsMessage);
                    OptionFocusState.LastOptionIndex = newIndex;
                }
            }
            catch (Exception ex) { MelonLogger.Error($"Error reading Option Focus (Language): {ex.Message}"); }
        }
    }
}

[HarmonyPatch(typeof(OptionLanguage), "UpdateValue")] 
public static class OptionLanguageUpdateValuePatch
{
    static void Postfix(OptionLanguage __instance)
    {
        try
        {
            int currentSelected = (int)AccessTools.Field(typeof(OptionLanguage), "mCurrentSelected").GetValue(__instance);
            System.Array containers = (System.Array)AccessTools.Field(typeof(OptionLanguage), "mLanguageChoice").GetValue(__instance);
            object currentContainer = containers.GetValue(currentSelected);

            object dataValue = AccessTools.Field(typeof(OptionLanguageContainer), "mDataValue").GetValue(currentContainer);
            string optionKey = dataValue.ToString();
            string ttsMessage = "";

            if (optionKey == "SUBTITLES")
            {
                Text subtitleTextComponent = (Text)AccessTools.Field(typeof(OptionLanguage), "mSubtitle").GetValue(__instance);
                ttsMessage = $"Subtitles. Changed to {subtitleTextComponent.text}.";
            }
            else if (optionKey == "COUNTRY")
            {
                Text countryTextComponent = (Text)AccessTools.Field(typeof(OptionLanguage), "mCountryTxt").GetValue(__instance);
                ttsMessage = $"Country. Changed to {countryTextComponent.text}.";
            }
            
            MelonLogger.Msg($"[TTS-OptionLanguage] Value Change: {ttsMessage}");
            TolkHelper.Speak(ttsMessage);
        }
        catch (Exception ex) { MelonLogger.Error($"Error reading value change (Language): {ex.Message}"); }
    }
}


// ==========================================================
// VIDEO-MENÜ PATCHES (VERTICALE UND WERTÄNDERUNG)
// ==========================================================
[HarmonyPatch(typeof(OptionVideo), "MoveInMenuSimple")] 
public static class OptionVideoNavigationPatch
{
    static void Postfix(OptionVideo __instance)
    {
        int newIndex = (int)AccessTools.Field(typeof(OptionVideo), "mCurrentSelected").GetValue(__instance);
        if (newIndex != -1 && newIndex != OptionFocusState.LastOptionIndex)
        {
            try
            {
                // ZUGRIFF ÜBER HILFSKLASSE
                Type containerType = OptionReflector.GetOptionVideoContainerType(); 
                System.Array containers = (System.Array)AccessTools.Field(typeof(OptionVideo), "mVideoChoice").GetValue(__instance);
                if (containers != null && containers.Length > newIndex)
                {
                    object currentContainer = containers.GetValue(newIndex);
                    object dataValue = AccessTools.Field(containerType, "mDataValue").GetValue(currentContainer);
                    string optionKey = dataValue.ToString();
                    
                    string currentValue = "Unknown";
                    
                    if (optionKey == "RESOLUTION")
                    {
                        Text resTextComponent = (Text)AccessTools.Field(typeof(OptionVideo), "mResolutionTxt").GetValue(__instance);
                        currentValue = resTextComponent.text;
                    }
                    else if (optionKey == "QUALITY")
                    {
                        Text qualityTextComponent = (Text)AccessTools.Field(typeof(OptionVideo), "mQualityTxt").GetValue(__instance);
                        currentValue = qualityTextComponent.text;
                    }
                    else if (optionKey == "DISPLAY")
                    {
                         Text fullscreenTextComponent = (Text)AccessTools.Field(typeof(OptionVideo), "mFullScreen").GetValue(__instance);
                        currentValue = fullscreenTextComponent.text;
                    }
                    
                    string ttsMessage = $"{optionKey}. Current value: {currentValue}."; 
                    
                    MelonLogger.Msg($"[TTS-OptionVideo] Focus: {ttsMessage}");
                    TolkHelper.Speak(ttsMessage);
                    OptionFocusState.LastOptionIndex = newIndex;
                }
            }
            catch (Exception ex) { MelonLogger.Error($"Error reading Option Focus (Video): {ex.Message}"); }
        }
    }
}

[HarmonyPatch(typeof(OptionVideo), "UpdateValue")] 
public static class OptionVideoUpdateValuePatch
{
    static void Postfix(OptionVideo __instance)
    {
        try
        {
            Type containerType = OptionReflector.GetOptionVideoContainerType(); // NEU
            int currentSelected = (int)AccessTools.Field(typeof(OptionVideo), "mCurrentSelected").GetValue(__instance);
            System.Array containers = (System.Array)AccessTools.Field(typeof(OptionVideo), "mVideoChoice").GetValue(__instance);
            object currentContainer = containers.GetValue(currentSelected);

            object dataValue = AccessTools.Field(containerType, "mDataValue").GetValue(currentContainer);
            string optionKey = dataValue.ToString();
            string ttsMessage = "";

            if (optionKey == "RESOLUTION")
            {
                Text resTextComponent = (Text)AccessTools.Field(typeof(OptionVideo), "mResolutionTxt").GetValue(__instance);
                ttsMessage = $"Resolution. Changed to {resTextComponent.text}.";
            }
            else if (optionKey == "QUALITY")
            {
                Text qualityTextComponent = (Text)AccessTools.Field(typeof(OptionVideo), "mQualityTxt").GetValue(__instance);
                ttsMessage = $"Quality. Changed to {qualityTextComponent.text}.";
            }
            else if (optionKey == "DISPLAY")
            {
                Text fullscreenTextComponent = (Text)AccessTools.Field(typeof(OptionVideo), "mFullScreen").GetValue(__instance);
                ttsMessage = $"Display Mode. Changed to {fullscreenTextComponent.text}.";
            }
            
            MelonLogger.Msg($"[TTS-OptionVideo] Value Change: {ttsMessage}");
            TolkHelper.Speak(ttsMessage);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error reading value change (Video): {ex.Message}");
        }
    }
}