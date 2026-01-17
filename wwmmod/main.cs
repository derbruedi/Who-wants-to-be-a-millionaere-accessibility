using MelonLoader;
using HarmonyLib;
using System;
using UnityEngine;

// ==========================================================
// 🚨 CRITICAL: MelonInfo Metadata
// ==========================================================
[assembly: MelonInfo(typeof(wwmmod), "WWTBAM TTS Mod", "1.0.0", "Atlan")]
[assembly: MelonGame("Microids", "Who Wants to Be a Millionaire? – New Edition")] 

public class wwmmod : MelonMod
{
    // ==========================================================
    // 📢 MOD START & TTS ACTIVATION
    // ==========================================================
    public override void OnApplicationStart()
    {
        MelonLogger.Msg("*******************************************");
        MelonLogger.Msg("🗣️ Who Wants to Be a Millionaire TTS Mod starting...");
        
        // <<< TTS ACTIVATION >>>
        try
        {
            TolkHelper.Load(); 
            TolkHelper.Speak("Text-to-Speech module loaded and activated. Good luck!");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"TTS module could not load Tolk. Error: {ex.Message}");
        }

        MelonLogger.Msg("Applying Harmony patches...");
    }

    // ==========================================================
    // ⌨️ KEY INPUT (Update Loop)
    // ==========================================================
    public override void OnUpdate()
    {
        // Check: Is 'M' pressed AND 'Alt' (Left or Right) held down?
        if (Input.GetKeyDown(KeyCode.M) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
        {
            MelonLogger.Msg("[TTS] Alt+M pressed - reading shop balance");
            ShopAccess.ReadCurrentMoney();
        }
    }

    // ==========================================================
    // 🧹 MOD EXIT & TTS DEACTIVATION
    // ==========================================================
    public override void OnApplicationQuit()
    {
        MelonLogger.Msg("TTS module shutting down.");
        
        // <<< TTS DEACTIVATION >>>
        TolkHelper.Unload(); 
    }
}