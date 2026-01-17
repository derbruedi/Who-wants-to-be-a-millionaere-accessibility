using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;
using System;

// ==========================================================
// GLOBAL HELPER AND ACCESS (Reflector)
// ==========================================================
public static class TTSFocusReflector
{
    // Stores the last state to detect changes
    public static int LastAnswerIndex = -1;
    public static int LastLifelineIndex = -1;
    public static bool LastIsLifelineContext = false;
    
    // Executes the logic for checking and outputting
    public static void CheckAndUpdateTTS(UIController uiController, bool isLifelineContext)
    {
        if (uiController == null) return;

        try
        {
            // Retrieve highlight data
            int currentAnswerIndex = uiController.mCurrentAnswerHighlighted;
            int currentLifelineIndex = uiController.mCurrentLifelineHighlighted;

            // Debugging focus change
            if (currentAnswerIndex != LastAnswerIndex || currentLifelineIndex != LastLifelineIndex || LastIsLifelineContext != isLifelineContext)
            {
                MelonLogger.Warning($"[TTS-DEBUG] Focus change detected! Answer: {currentAnswerIndex}, Lifeline: {currentLifelineIndex}, Lifeline Context: {isLifelineContext}");
            }

            // ----------------------------------------------------
            // 1. LIFELINE CONTEXT
            // ----------------------------------------------------
            if (isLifelineContext)
            {
                if (currentLifelineIndex != -1 && (currentLifelineIndex != LastLifelineIndex || !LastIsLifelineContext))
                {
                    // FIX 1: Casting to System.Array for type-safe access
                    System.Array lifelines = (System.Array)AccessTools.Field(typeof(UIController), "mLifelines")?.GetValue(uiController);
                    
                    if (lifelines != null && currentLifelineIndex < lifelines.Length)
                    {
                        object lifelineStruct = lifelines.GetValue(currentLifelineIndex);
                        
                        // Get field mValue from inner Lifeline structure
                        FieldInfo lifelineValueField = AccessTools.Inner(typeof(UIController), "Lifeline")?.GetField("mValue");
                        
                        if (lifelineValueField != null)
                        {
                            object lifelineEnumValue = lifelineValueField.GetValue(lifelineStruct);
                            
                            string lifelineName = lifelineEnumValue.ToString().Replace('_', ' ');
                            
                            string ttsMessage = $"Lifeline selected: {lifelineName}";
                            
                            MelonLogger.Msg($"[TTS-Game] New Lifeline: {ttsMessage}");
                            TolkHelper.Speak(ttsMessage);
                        } else {
                            MelonLogger.Error("[TTS-DEBUG] Error: Could not find field 'mValue' in Lifeline structure.");
                        }
                    }
                }
                LastLifelineIndex = currentLifelineIndex;
            }
            // ----------------------------------------------------
            // 2. ANSWER CONTEXT
            // ----------------------------------------------------
            else
            {
                if (currentAnswerIndex != -1 && (currentAnswerIndex != LastAnswerIndex || LastIsLifelineContext))
                {
                    // FIX 1: Casting to System.Array for type-safe access
                    System.Array answers = (System.Array)AccessTools.Field(typeof(UIController), "mAnswers")?.GetValue(uiController);
                    
                    if (answers != null && currentAnswerIndex < answers.Length)
                    {
                        object answerStruct = answers.GetValue(currentAnswerIndex);
                        
                        // Get field mAnswer from inner Answer structure
                        FieldInfo answerTextField = AccessTools.Inner(typeof(UIController), "Answer")?.GetField("mAnswer");
                        
                        if (answerTextField != null)
                        {
                            object answerTextObject = answerTextField.GetValue(answerStruct);

                            // FIX 2: Specific Debugging
                            if (answerTextObject == null)
                            {
                                MelonLogger.Error($"[TTS-DEBUG] Text object (A:{currentAnswerIndex}) is NULL.");
                                return;
                            }
                            
                            // Try casting to UnityEngine.UI.Text
                            if (answerTextObject is UnityEngine.UI.Text answerTextComponent)
                            {
                                if (answerTextComponent != null && answerTextComponent.color.a > 0.5f)
                                {
                                    string answerText = answerTextComponent.text;
                                    // Clean text from special characters at start
                                    string cleanedText = new string(answerText.SkipWhile(c => !char.IsLetterOrDigit(c)).ToArray());
                                    
                                    // --- NEW: Determine Letter ---
                                    string letterPrefix = "";
                                    switch (currentAnswerIndex)
                                    {
                                        case 0: letterPrefix = "A"; break;
                                        case 1: letterPrefix = "B"; break;
                                        case 2: letterPrefix = "C"; break;
                                        case 3: letterPrefix = "D"; break;
                                        default: letterPrefix = "Answer"; break;
                                    }

                                    // --- NEW: Assemble Message (e.g. "A: 500 Euro") ---
                                    string ttsMessage = $"{letterPrefix}: {cleanedText}";
                                    
                                    MelonLogger.Msg($"[TTS-Game] New Answer: {ttsMessage}");
                                    TolkHelper.Speak(ttsMessage);
                                } else {
                                    MelonLogger.Error($"[TTS-DEBUG] Text component (A:{currentAnswerIndex}) not visible or NULL.");
                                }
                            }
                            else
                            {
                                MelonLogger.Error($"[TTS-DEBUG] FATAL: Wrong Type! Expected: UnityEngine.UI.Text. Found: {answerTextObject.GetType().FullName}");
                            }
                        } else {
                            MelonLogger.Error("[TTS-DEBUG] Error: Could not find field 'mAnswer' in Answer structure.");
                        }
                    }
                }
                LastAnswerIndex = currentAnswerIndex;
            }
            
            LastIsLifelineContext = isLifelineContext;
        }
        catch (System.Exception ex)
        {
            MelonLogger.Error($"[TTS-DEBUG] Fatal error in CheckAndUpdateTTS: {ex.Message}");
        }
    }
}


// ==========================================================
// PATCH: UIController.Update()
// ==========================================================
[HarmonyPatch(typeof(UIController))]
public static class UIControllerUpdatePatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        return AccessTools.DeclaredMethod(typeof(UIController), "Update");
    }

    static void Postfix(UIController __instance)
    {
        bool isPyramidShowing = __instance.PyramidShowing();
        TTSFocusReflector.CheckAndUpdateTTS(__instance, isPyramidShowing);
    }
}

[HarmonyPatch(typeof(AnswerStep), "AnswerSelectionButton")]
public static class AnswerStep_AnswerSelectionButton_Patch
{
    static bool Prefix()
    {
        if (PauseState.IsPaused)
        {
            return false; // Skip original method if paused
        }
        return true; // Execute original method
    }
}

[HarmonyPatch(typeof(AnswerStep), "PyramidShowHideButton")]
public static class AnswerStep_PyramidShowHideButton_Patch
{
    static bool Prefix()
    {
        if (PauseState.IsPaused)
        {
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(AnswerStep), "LegendShowHideButton")]
public static class AnswerStep_LegendShowHideButton_Patch
{
    static bool Prefix()
    {
        if (PauseState.IsPaused)
        {
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(AnswerStep), "LifelineSelectionButton")]
public static class AnswerStep_LifelineSelectionButton_Patch
{
    static bool Prefix()
    {
        if (PauseState.IsPaused)
        {
            return false;
        }
        return true;
    }
}