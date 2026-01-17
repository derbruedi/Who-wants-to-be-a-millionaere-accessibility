using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

// ==========================================================
// PATCH: SimpleOption (Timer Enable/Disable & Other Gameplay Options)
// This handles the screen where players choose timer on/off, difficulty, etc.
// ==========================================================

public static class SimpleOptionState
{
    public static int LastSelectedIndex = -1;
}

// Patch 1: Read option when navigating with keyboard/controller
[HarmonyPatch(typeof(SimpleOption), "MoveInMenuSimple")]
public static class SimpleOptionNavigationPatch
{
    static void Postfix(SimpleOption __instance)
    {
        try
        {
            int currentSelected = (int)AccessTools.Field(typeof(SimpleOption), "mCurrentSelected").GetValue(__instance);
            int oldSelected = (int)AccessTools.Field(typeof(SimpleOption), "mOldSelected").GetValue(__instance);

            // Only speak if selection changed
            if (currentSelected != oldSelected)
            {
                ReadCurrentOption(__instance, currentSelected);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-SimpleOption] Error in MoveInMenuSimple patch: {ex.Message}");
        }
    }

    public static void ReadCurrentOption(SimpleOption instance, int index)
    {
        try
        {
            var choicesField = AccessTools.Field(typeof(SimpleOption), "mGameplayChoices");
            Array choices = (Array)choicesField.GetValue(instance);

            if (choices != null && index >= 0 && index < choices.Length)
            {
                object container = choices.GetValue(index);
                Type containerType = container.GetType();

                // Get the text label
                var textField = containerType.GetField("mText");
                Text textComponent = (Text)textField.GetValue(container);

                // Get the data value for additional context
                var dataField = containerType.GetField("mDataValue");
                object dataValue = dataField.GetValue(container);

                string optionText = textComponent != null ? textComponent.text : "Unknown";
                string dataString = dataValue.ToString();

                // Make common options more descriptive
                string ttsMessage = optionText;

                // Add context for timer options
                if (dataString == "NO_TIMER" || dataString == "TIMER")
                {
                    ttsMessage = dataString == "TIMER" ? $"{optionText} (Timer enabled)" : $"{optionText} (No timer)";
                }

                MelonLogger.Msg($"[TTS-SimpleOption] Selected: {ttsMessage} (Value: {dataString})");
                TolkHelper.Speak(ttsMessage);

                SimpleOptionState.LastSelectedIndex = index;
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-SimpleOption] Error reading option: {ex.Message}");
        }
    }
}

// Patch 2: Read option when highlighted (covers mouse selection and initial display)
[HarmonyPatch(typeof(SimpleOption), "HighlightSelected")]
public static class SimpleOptionHighlightPatch
{
    static void Postfix(SimpleOption __instance)
    {
        try
        {
            int currentSelected = (int)AccessTools.Field(typeof(SimpleOption), "mCurrentSelected").GetValue(__instance);

            // Only speak if this is a new selection
            if (currentSelected != SimpleOptionState.LastSelectedIndex)
            {
                SimpleOptionNavigationPatch.ReadCurrentOption(__instance, currentSelected);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-SimpleOption] Error in HighlightSelected patch: {ex.Message}");
        }
    }
}

// Patch 3: Announce when screen appears
[HarmonyPatch(typeof(SimpleOption), "StartStep")]
public static class SimpleOptionStartPatch
{
    static void Postfix(SimpleOption __instance)
    {
        try
        {
            // Reset state
            SimpleOptionState.LastSelectedIndex = -1;

            // Get the selection mode to provide context
            var modeField = AccessTools.Field(typeof(SimpleOption), "mSelectionMode");
            object selectionMode = modeField.GetValue(__instance);
            string modeString = selectionMode.ToString();

            string screenName = "Options";
            if (modeString.Contains("OPTION"))
            {
                screenName = "Timer options";
            }
            else if (modeString.Contains("DIFFICULTY"))
            {
                screenName = "Difficulty selection";
            }
            else if (modeString.Contains("MODE"))
            {
                screenName = "Game mode selection";
            }

            MelonLogger.Msg($"[TTS-SimpleOption] Screen opened: {screenName}");
            TolkHelper.Speak($"{screenName}. Use up and down to navigate, Enter to select.");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-SimpleOption] Error in StartStep patch: {ex.Message}");
        }
    }
}
