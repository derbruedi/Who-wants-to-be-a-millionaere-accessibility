using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

// ==========================================================
// PATCH: PackSelection (Theme Pack Selection Before Game Start)
// Now correctly identifies: Purchased vs Not Purchased packs
// ==========================================================

public static class PackSelectionState
{
    public static int LastPackIndex = -1;
    public static bool LastWasContinueButton = false;
}

// Patch 1: Read pack info when navigating
[HarmonyPatch(typeof(PackSelection), "HighlightSelected")]
public static class PackHighlightPatch
{
    static void Postfix(PackSelection __instance)
    {
        // Don't read if game is paused
        // Pause check removed

        try
        {
            // Get current index
            FieldInfo indexField = AccessTools.Field(typeof(PackSelection), "mCurrentIndex");
            int currentIndex = (int)indexField.GetValue(__instance);

            // Check if on Continue button (index -1)
            bool onContinueButton = (currentIndex == -1);

            // Check if selection changed
            if (currentIndex == PackSelectionState.LastPackIndex &&
                onContinueButton == PackSelectionState.LastWasContinueButton)
            {
                return; // No change, don't repeat
            }

            PackSelectionState.LastPackIndex = currentIndex;
            PackSelectionState.LastWasContinueButton = onContinueButton;

            // Handle Continue button
            if (onContinueButton)
            {
                // Check if Continue is available (enough packs selected)
                FieldInfo selectedField = AccessTools.Field(typeof(PackSelection), "mSelected");
                FieldInfo minField = AccessTools.Field(typeof(PackSelection), "mMinToSelect");
                int selected = (int)selectedField.GetValue(__instance);
                int minRequired = (int)minField.GetValue(__instance);

                if (selected >= minRequired)
                {
                    TolkHelper.Speak("Continue. Press Enter to start the game.");
                }
                else
                {
                    TolkHelper.Speak($"Continue button. Select at least {minRequired} packs to continue.");
                }
                return;
            }

            // Get pack list
            FieldInfo packsField = AccessTools.Field(typeof(PackSelection), "mAllPacks");
            Array packs = (Array)packsField.GetValue(__instance);

            if (packs != null && currentIndex >= 0 && currentIndex < packs.Length)
            {
                // Get the SimplePackHandler
                object packHandler = packs.GetValue(currentIndex);

                // Read pack properties directly since they are public
                SimplePackHandler handler = packHandler as SimplePackHandler;
                if (handler != null)
                {
                    string packName = handler.mText != null ? handler.mText.text : "Unknown Pack";
                    bool isBought = handler.mBought;
                    bool isSelected = handler.mSelected;

                    // Build status message
                    string status;
                    if (!isBought)
                    {
                        // Not purchased - this is the critical info users need!
                        status = "Not purchased. Press Enter to buy.";
                    }
                    else if (isSelected)
                    {
                        status = "Purchased. Selected for play.";
                    }
                    else
                    {
                        status = "Purchased. Not selected.";
                    }

                    string ttsMessage = $"{packName}. {status}";
                    MelonLogger.Msg($"[TTS-PackSelect] {ttsMessage}");
                    TolkHelper.Speak(ttsMessage);
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-PackSelect] Error: {ex.Message}");
        }
    }
}

// Patch 2: Announce when pack is activated (selected for play)
[HarmonyPatch(typeof(SimplePackHandler), "Activate")]
public static class PackActivatePatch
{
    static void Postfix(SimplePackHandler __instance)
    {
        // Pause check removed

        try
        {
            string name = __instance.mText != null ? __instance.mText.text : "Pack";
            TolkHelper.Speak($"{name} selected for play.");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-PackSelect] Activate error: {ex.Message}");
        }
    }
}

// Patch 3: Announce when pack is deactivated (unselected)
[HarmonyPatch(typeof(SimplePackHandler), "DeActivate")]
public static class PackDeactivatePatch
{
    static void Postfix(SimplePackHandler __instance)
    {
        // Pause check removed

        try
        {
            string name = __instance.mText != null ? __instance.mText.text : "Pack";
            TolkHelper.Speak($"{name} unselected.");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-PackSelect] Deactivate error: {ex.Message}");
        }
    }
}

// Patch 4: Announce when pack is purchased
[HarmonyPatch(typeof(SimplePackHandler), "Buy")]
public static class PackBuyPatch
{
    static void Postfix(SimplePackHandler __instance)
    {
        // Pause check removed

        try
        {
            string name = __instance.mText != null ? __instance.mText.text : "Pack";
            TolkHelper.Speak($"{name} purchased and selected for play.");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-PackSelect] Buy error: {ex.Message}");
        }
    }
}

// Patch 5: Announce buy popup navigation
[HarmonyPatch(typeof(PackSelection), "HighlightBuy")]
public static class PackBuyHighlightPatch
{
    private static bool lastToBuy = false;
    private static bool firstCall = true;

    static void Postfix(PackSelection __instance)
    {
        // Pause check removed

        try
        {
            FieldInfo toBuyField = AccessTools.Field(typeof(PackSelection), "mToBuy");
            bool toBuy = (bool)toBuyField.GetValue(__instance);

            // Speak on first call or when selection changes
            if (firstCall || toBuy != lastToBuy)
            {
                firstCall = false;
                lastToBuy = toBuy;

                string selection = toBuy ? "Buy. Confirm purchase." : "Cancel. Do not buy.";
                TolkHelper.Speak(selection);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[TTS-PackSelect] HighlightBuy error: {ex.Message}");
        }
    }

    // Reset state when popup opens
    [HarmonyPatch(typeof(PackSelection), "ResetHighlightBuy")]
    public static class ResetBuyStatePatch
    {
        static void Postfix()
        {
            firstCall = true;
            lastToBuy = false;
        }
    }
}

// Patch 6: Announce when pack selection screen opens
[HarmonyPatch(typeof(PackSelection), "StartStep")]
public static class PackSelectionStartPatch
{
    static void Postfix()
    {
        // Pause check removed

        // Reset state
        PackSelectionState.LastPackIndex = -1;
        PackSelectionState.LastWasContinueButton = false;

        MelonLogger.Msg("[TTS-PackSelect] Screen opened");
        TolkHelper.Speak("Pack selection. Choose question packs for the game. Use arrow keys to navigate, Enter to select or buy.");
    }
}
