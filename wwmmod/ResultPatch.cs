using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Linq;

// ==========================================================
// PATCH: Announces the correct answer after losing.
// ==========================================================
[HarmonyPatch(typeof(StageResultStep), "StartDefeatResult")]
public static class AnnounceCorrectAnswerPatch
{
    static void Postfix(StageResultStep __instance)
    {
        try
        {
            UIController uiController = UIController.sInstance;
            if (uiController == null)
            {
                MelonLogger.Warning("[AnnounceCorrectAnswer] UIController instance is null.");
                return;
            }

            // Get the correct answer index via the public method
            int correctIndex = uiController.GetRightAnswerValue();

            // Now get the answer text from the mAnswers array
            var answersField = AccessTools.Field(typeof(UIController), "mAnswers");
            Array answers = (Array)answersField.GetValue(uiController);

            if (answers != null && correctIndex >= 0 && correctIndex < answers.Length)
            {
                object answerStruct = answers.GetValue(correctIndex);
                var answerTextField = answerStruct.GetType().GetField("mAnswer");
                Text answerTextComponent = (Text)answerTextField.GetValue(answerStruct);

                if (answerTextComponent != null)
                {
                    string answerText = answerTextComponent.text;
                    string cleanedText = new string(answerText.SkipWhile(c => !char.IsLetterOrDigit(c)).ToArray());

                    string ttsMessage = $"The correct answer was: {cleanedText}";
                    MelonLogger.Msg($"[AnnounceCorrectAnswer] Announcing: {ttsMessage}");
                    TolkHelper.Speak(ttsMessage);
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[AnnounceCorrectAnswer] Error in patch: {ex.Message}");
        }
    }
}
