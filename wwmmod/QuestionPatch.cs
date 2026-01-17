using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI; 
using System.Reflection;
using System;

// ==========================================================
// Speichert den Status der Fragen (damit wir nicht doppelt lesen)
// ==========================================================
public static class QuestionState
{
    public static string LastReadQuestion = "";
}

// ==========================================================
// PATCH: Liest die Frage aus UIController aus
// ==========================================================
[HarmonyPatch(typeof(UIController), "Update")]
public static class QuestionReaderPatch
{
    // We use Postfix so we run after the game has done its update
    static void Postfix(UIController __instance)
    {
        // 1. Manual repeat with 'R' key
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!string.IsNullOrEmpty(QuestionState.LastReadQuestion))
            {
                MelonLogger.Msg("[TTS-Question] Manuelle Wiederholung.");
                TolkHelper.Speak($"Question: {QuestionState.LastReadQuestion}");
            }
            return;
        }

        // 2. Automatische Erkennung neuer Fragen
        try 
        {
            string currentText = GetQuestionText(__instance);

            // Wenn Text gefunden wurde, er nicht leer ist, und anders als beim letzten Mal
            if (!string.IsNullOrEmpty(currentText) && currentText != QuestionState.LastReadQuestion)
            {
                // Kurze Pause oder Check, ob der Text lang genug ist (um flackern zu vermeiden)
                if (currentText.Length > 2) 
                {
                    MelonLogger.Msg($"[TTS-Question] Neue Frage erkannt: {currentText}");
                    TolkHelper.Speak($"New Question: {currentText}");
                    
                    // Speichern, damit wir es nicht jeden Frame vorlesen
                    QuestionState.LastReadQuestion = currentText;
                }
            }
        }
        catch (Exception ex)
        {
            // Fehler fangen, aber nicht spammen (passiert evtl. wenn kein Spiel läuft)
        }
    }

    // ==========================================================
    // HELFER: Versucht das Textfeld für die Frage zu finden
    // ==========================================================
    static string GetQuestionText(UIController uiController)
    {
        if (uiController == null) return "";

        // Strategie 1: Suche nach einem Feld namens "mQuestion"
        FieldInfo questionField = AccessTools.Field(typeof(UIController), "mQuestion");
        if (questionField == null) return "";

        object questionObj = questionField.GetValue(uiController);
        if (questionObj == null) return "";

        // Fall A: Es ist direkt ein Text-Objekt
        if (questionObj is Text textComponent)
        {
            // Nur lesen, wenn der Text auch sichtbar ist (Alpha > 0)
            if (textComponent.color.a > 0.1f)
            {
                return textComponent.text;
            }
            return "";
        }

        // Fall B: Es ist eine Struktur/Klasse, die den Text enthält
        // Wir schauen per Reflection in das Objekt rein
        Type innerType = questionObj.GetType();
        
        // Suche nach typischen Namen für Textfelder
        FieldInfo innerTextField = AccessTools.Field(innerType, "mText") ?? 
                                   AccessTools.Field(innerType, "text") ??
                                   AccessTools.Field(innerType, "mString");

        if (innerTextField != null)
        {
            object innerValue = innerTextField.GetValue(questionObj);
            
            // Wenn das innere Feld ein Unity-Text ist
            if (innerValue is Text innerTextComp) return innerTextComp.text;
            // Wenn das innere Feld direkt ein String ist
            if (innerValue is string innerString) return innerString;
        }

        return "";
    }
}