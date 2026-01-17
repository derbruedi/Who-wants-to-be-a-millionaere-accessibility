using HarmonyLib;
using MelonLoader;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// ==========================================================
// PATCH: Online Status (Taste P)
// Sagt: "Rang 5 von 99. 12 Spieler eliminiert."
// ==========================================================
[HarmonyPatch(typeof(OnlineGameplay), "Update")]
public static class OnlineStatusPatch
{
    static void Postfix(OnlineGameplay __instance)
    {
        // Key U for online status (Rank/Position)
        if (Input.GetKeyDown(KeyCode.U))
        {
            SpeakOnlineStatus(__instance);
        }
    }

    static void SpeakOnlineStatus(OnlineGameplay game)
    {
        try
        {
            // 1. Gesamtzahl der Spieler
            int totalPlayers = game.mPlayerCount;
            
            // 2. Eigener Rang (z.B. Platz 15)
            // Hinweis: mCurrentPos ist privat, wir holen es per Reflection
            int myRank = (int)AccessTools.Field(typeof(OnlineGameplay), "mCurrentPos").GetValue(game);

            // 3. Eliminierte Spieler zählen
            // Das ist etwas komplizierter, weil wir tief in die Lobby-Daten greifen müssen
            int eliminatedCount = -1;
            int remainingCount = -1;

            try 
            {
                // Hole das Lobby-Objekt
                object lobby = AccessTools.Field(typeof(OnlineGameplay), "mOnlineLobby").GetValue(game);
                if (lobby != null)
                {
                    // Hole das Dictionary mit allen Spielern (mOnlinePlayers)
                    // Da wir den Typ "OnlinePlayer" nicht kennen, nutzen wir 'dynamic' oder Reflection auf das Dictionary
                    System.Collections.IDictionary playersDict = (System.Collections.IDictionary)AccessTools.Field(lobby.GetType(), "mOnlinePlayers").GetValue(lobby);
                    
                    if (playersDict != null)
                    {
                        int lostCounter = 0;
                        
                        // Wir iterieren durch alle Spieler
                        foreach (object playerEntry in playersDict.Values)
                        {
                            // Wir suchen nach einem Feld "mLost" im Spieler-Objekt
                            // Das Feld heißt in OnlineGameplay "mLost", also wahrscheinlich auch im Spieler-Objekt
                            FieldInfo lostField = AccessTools.Field(playerEntry.GetType(), "mLost");
                            if (lostField != null)
                            {
                                bool hasLost = (bool)lostField.GetValue(playerEntry);
                                if (hasLost)
                                {
                                    lostCounter++;
                                }
                            }
                        }
                        eliminatedCount = lostCounter;
                        remainingCount = totalPlayers - eliminatedCount;
                    }
                }
            }
            catch (Exception ex)
            {
                // Falls das Zählen der Eliminierten schiefgeht (z.B. Feldname anders), ignorieren wir es
                MelonLogger.Warning($"[TTS-Online] Konnte Eliminierte nicht zählen: {ex.Message}");
            }

            // --- Nachricht zusammenbauen ---
            string message = "";

            if (myRank > 0)
            {
                message += $"Rang: {myRank} von {totalPlayers}. ";
            }
            else
            {
                message += $"Spieler: {totalPlayers}. ";
            }

            if (eliminatedCount >= 0)
            {
                message += $"Eliminiert: {eliminatedCount}. Verbleibend: {remainingCount}.";
            }
            else
            {
                // Fallback, falls wir Eliminierte nicht zählen konnten
                message += "Status unbekannt.";
            }

            MelonLogger.Msg($"[TTS-Online] {message}");
            TolkHelper.Speak(message);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Fehler im Online-Patch: {ex.Message}");
            TolkHelper.Speak("Online Status Fehler");
        }
    }
}