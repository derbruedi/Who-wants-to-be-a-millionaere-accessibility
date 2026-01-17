using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq; // WICHTIG: Neu hinzugefügt für Listen-Suche

// ==========================================================
// PATCH: Spezial-Menüs (Telefonjoker & Charakter) V5 (FIXED)
// Feature: Taste "I" sucht nun aktiv den sichtbaren Kandidaten
// ==========================================================

public static class SpecialMenuPatch
{
    // --- 1. TELEFONJOKER (Funktioniert bereits, unverändert) ---
    [HarmonyPatch(typeof(UIController), "MoveInCallAFriend")]
    public static class PhoneAFriendPatch
    {
        static void Postfix(UIController __instance)
        {
            try
            {
                if (__instance.mFriendsContainer != null)
                {
                    foreach (var friend in __instance.mFriendsContainer)
                    {
                        if (friend.mHighlight != null && friend.mHighlight.enabled)
                        {
                            string name = friend.mName != null ? friend.mName.text : "";
                            string job = friend.mJob != null ? friend.mJob.text : "";
                            string textToSpeak = $"{name}, {job}";
                            
                            MelonLogger.Msg($"[TTS-Phone] Freund: {textToSpeak}");
                            TolkHelper.Speak(textToSpeak);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex) { MelonLogger.Error($"Fehler Telefonjoker: {ex.Message}"); }
        }
    }

    // --- 2. CHARAKTER KANDIDAT NAME BEIM WECHSELN ---
    // Dies liest nur den Namen vor, wenn man durchschaltet.
    [HarmonyPatch(typeof(CharacterCandidate), "OnEnable")]
    public static class CandidateEnablePatch
    {
        static void Postfix(CharacterCandidate __instance)
        {
            try
            {
                // Sicherheitscheck: Nur vorlesen, wenn das Objekt wirklich sichtbar im Baum ist
                if (!__instance.gameObject.activeInHierarchy) return;

                if (Shell.sInstance == null || Shell.sInstance.mMenuDatas == null) return;

                var charEnum = __instance.GetCandidateValue();
                eAllCandidate castedEnum = (eAllCandidate)(int)charEnum;

                string translatedName = Shell.sInstance.mMenuDatas.GetCandidateTranslation(castedEnum);

                if (string.IsNullOrEmpty(translatedName))
                {
                    translatedName = charEnum.ToString().Replace("_", " ");
                }

                MelonLogger.Msg($"[TTS-Char] Kandidat gewählt: {translatedName}");
                TolkHelper.Speak(translatedName);
            }
            catch {}
        }
    }

    // --- 3. Key "I" to read candidate bio ---
    [HarmonyPatch(typeof(UIController), "Update")]
    public static class CandidateInfoKeyPatch
    {
        static void Postfix()
        {
            // Check if I key is pressed
            if (Input.GetKeyDown(KeyCode.I))
            {
                MelonLogger.Msg("[TTS-Debug] Taste I gedrückt. Suche aktiven Kandidaten...");
                SpeakActiveCandidateBio();
            }
        }

        static void SpeakActiveCandidateBio()
        {
            try
            {
                // 1. Alle Kandidaten-Objekte in der Szene finden
                var allCandidates = GameObject.FindObjectsOfType<CharacterCandidate>();

                // 2. Denjenigen finden, der AKTIV und SICHTBAR ist
                // Wir nehmen den ersten, der "activeInHierarchy" ist.
                CharacterCandidate activeCandidate = null;

                foreach (var cand in allCandidates)
                {
                    if (cand.gameObject.activeInHierarchy && cand.enabled)
                    {
                        activeCandidate = cand;
                        break; // Gefunden!
                    }
                }

                if (activeCandidate == null)
                {
                    MelonLogger.Msg("[TTS-Debug] Kein aktiver Kandidat gefunden (Menu offen?).");
                    return;
                }

                // 3. Daten holen und sprechen
                if (Shell.sInstance == null || Shell.sInstance.mMenuDatas == null) return;

                var charEnum = activeCandidate.GetCandidateValue();
                eAllCandidate castedEnum = (eAllCandidate)(int)charEnum;

                string name = "", job = "", hobby = "";
                // Die Methode füllt die Variablen name, job, hobby
                Shell.sInstance.mMenuDatas.GetCandidateAllInfoTranslation(out name, out job, out hobby, castedEnum);

                string fullText = $"Name: {name}. Beruf: {job}. Hobby: {hobby}.";
                
                MelonLogger.Msg($"[TTS-Info] {fullText}");
                TolkHelper.Speak(fullText);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Fehler bei Bio Vorlesen: {ex.Message}");
            }
        }
    }
}