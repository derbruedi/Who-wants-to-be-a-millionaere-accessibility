using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

// ==========================================================
// PATCH: Pause Menu
// Provides narration for pause menu and blocks game UI reading while paused
// ==========================================================

public static class PauseState
{
    // Track if game is paused - other patches should check this!
    public static bool IsPaused = false;
    public static bool LastPauseInYes = false;

    // Helper method for other patches to check pause state
    public static bool ShouldSuppressTTS()
    {
        return IsPaused;
    }
}

// Patch 1: Announce when pause is activated
[HarmonyPatch(typeof(UIController), "ActivatePause")]
public static class PauseActivatePatch
{
    static void Postfix(UIController __instance)
    {
        try
        {
            PauseState.IsPaused = true;
            PauseState.LastPauseInYes = false;

            // Check if this is a controller missing pause or regular pause
            var controllerTextField = AccessTools.Field(typeof(UIController), "mControllerText");
            object controllerText = controllerTextField?.GetValue(__instance);

            // Get alpha via reflection to avoid type dependency
            float controllerAlpha = 0f;
            if (controllerText != null)
            {
                var alphaProperty = controllerText.GetType().GetProperty("alpha");
                if (alphaProperty != null)
                {
                    controllerAlpha = (float)alphaProperty.GetValue(controllerText);
                }
            }

            string message;
            if (controllerText != null && controllerAlpha > 0.5f)
            {
                message = "Controller disconnected. Select Resume to continue or Leave to exit.";
            }
            else
            {
                message = "Game paused. Resume game, or Leave game. Use left and right to navigate, Enter to select.";
            }

            MelonLogger.Msg($"[TTS-Pause] Pause activated");
            TolkHelper.Speak(message);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-Pause] Error in ActivatePause patch: {ex.Message}");
        }
    }
}

// Patch 2: Announce when pause is deactivated
[HarmonyPatch(typeof(UIController), "DeactivatePause")]
public static class PauseDeactivatePatch
{
    static void Postfix(UIController __instance)
    {
        try
        {
            PauseState.IsPaused = false;

            MelonLogger.Msg($"[TTS-Pause] Pause deactivated");
            TolkHelper.Speak("Game resumed.");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-Pause] Error in DeactivatePause patch: {ex.Message}");
        }
    }
}

// Patch 3: Announce navigation between Resume/Leave
[HarmonyPatch(typeof(UIController), "HighlightPause")]
public static class PauseHighlightPatch
{
    static void Postfix(UIController __instance)
    {
        try
        {
            // Get current selection (mPauseInYes: false = Resume/No, true = Leave/Yes)
            var pauseInYesField = AccessTools.Field(typeof(UIController), "mPauseInYes");
            bool pauseInYes = (bool)pauseInYesField.GetValue(__instance);

            // Only speak if selection changed
            if (pauseInYes != PauseState.LastPauseInYes || !PauseState.IsPaused)
            {
                PauseState.LastPauseInYes = pauseInYes;

                // Get the actual button text from the UI
                var popupAnswerField = AccessTools.Field(typeof(UIController), "mPausePopupAnswer");
                Array popupAnswers = (Array)popupAnswerField.GetValue(__instance);

                if (popupAnswers != null && popupAnswers.Length >= 2)
                {
                    // Index 0 = Resume (No), Index 1 = Leave (Yes)
                    int selectedIndex = pauseInYes ? 1 : 0;
                    object selectedAnswer = popupAnswers.GetValue(selectedIndex);

                    var textField = selectedAnswer.GetType().GetField("mText");
                    Text textComponent = (Text)textField.GetValue(selectedAnswer);

                    string buttonText = textComponent != null ? textComponent.text : (pauseInYes ? "Leave game" : "Resume game");

                    MelonLogger.Msg($"[TTS-Pause] Selection: {buttonText}");
                    TolkHelper.Speak(buttonText);
                }
                else
                {
                    // Fallback if we can't read the UI
                    string selection = pauseInYes ? "Leave game" : "Resume game";
                    TolkHelper.Speak(selection);
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-Pause] Error in HighlightPause patch: {ex.Message}");
        }
    }
}

// Note: PauseState.IsPaused is reset in DeactivatePause patch
