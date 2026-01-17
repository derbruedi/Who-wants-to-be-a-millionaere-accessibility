using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

// ==========================================================
// PATCH: MenuShop (Wer wird Millionär - Shop/Store)
// ==========================================================

public static class ShopAccess
{
    // Merker für den letzten Index, damit er nicht doppelt spricht
    private static int lastSelectedIndex = -1;

    // --- PATCH 1: Automatisch vorlesen beim Blättern im Shop ---
    [HarmonyPatch(typeof(MenuShop), "HighlightSelected")]
    public static class HighlightSelectedPatch
    {
        static void Postfix(MenuShop __instance)
        {
            try
            {
                // 1. Aktuellen Index holen (private Variable mCurrentSelected)
                FieldInfo selectedField = AccessTools.Field(typeof(MenuShop), "mCurrentSelected");
                int currentIndex = (int)selectedField.GetValue(__instance);

                // Nur sprechen, wenn sich die Auswahl geändert hat
                if (currentIndex == lastSelectedIndex) return;
                lastSelectedIndex = currentIndex;

                ReadShopItem(__instance, currentIndex);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Shop TTS Error: {ex.Message}");
            }
        }
    }

    // --- PATCH 2: Vorlesen beim "Kaufen?" Popup (Ja/Nein) ---
    [HarmonyPatch(typeof(MenuShop), "HighlightBuy")]
    public static class HighlightBuyPatch
    {
        static void Postfix(MenuShop __instance)
        {
            try 
            {
                // Variable mToBuy (bool) bestimmt, ob man auf JA oder NEIN steht
                FieldInfo buyField = AccessTools.Field(typeof(MenuShop), "mToBuy");
                bool toBuy = (bool)buyField.GetValue(__instance);

                string text = toBuy ? "Buy (Yes)" : "Cancel (No)";
                TolkHelper.Speak(text);
            }
            catch {}
        }
    }

    // --- HILFSFUNKTION: Liest Name und Preis aus ---
    public static void ReadShopItem(MenuShop shopInstance, int index)
    {
        try
        {
            // 1. Array der Pakete holen (mShopChoices)
            FieldInfo choicesField = AccessTools.Field(typeof(MenuShop), "mShopChoices");
            Array choices = (Array)choicesField.GetValue(shopInstance);

            if (choices != null && index < choices.Length && index >= 0)
            {
                // Das einzelne Paket-Objekt (Struct ShopPackContainer) holen
                object packContainer = choices.GetValue(index);
                Type packType = packContainer.GetType();

                // 2. Name des Pakets (mText)
                FieldInfo textField = packType.GetField("mText");
                Text textComponent = (Text)textField.GetValue(packContainer);
                string packName = textComponent != null ? textComponent.text : "Unknown Item";

                // 3. Prüfen ob Gekauft (mBoughtImages)
                bool isBought = false;
                FieldInfo boughtField = packType.GetField("mBoughtImages");
                Image[] boughtImages = (Image[])boughtField.GetValue(packContainer);
                
                if (boughtImages != null)
                {
                    foreach (var img in boughtImages)
                    {
                        if (img != null && img.enabled)
                        {
                            isBought = true;
                            break;
                        }
                    }
                }

                // 4. Preis holen (aus mShopInfos -> mPriceAmount)
                FieldInfo infosField = AccessTools.Field(typeof(MenuShop), "mShopInfos");
                object shopInfos = infosField.GetValue(shopInstance);
                
                string price = "";
                if (shopInfos != null)
                {
                    Type infoType = shopInfos.GetType();
                    FieldInfo priceField = infoType.GetField("mPriceAmount");
                    Text priceTextComp = (Text)priceField.GetValue(shopInfos);
                    if (priceTextComp != null)
                    {
                        price = priceTextComp.text;
                    }
                }

                // Nachricht zusammenbauen
                string ttsMessage = $"{packName}.";
                
                if (isBought)
                {
                    ttsMessage += " Purchased.";
                }
                else if (!string.IsNullOrEmpty(price))
                {
                    ttsMessage += $" Price: {price}";
                }

                MelonLogger.Msg($"[Shop-TTS] {ttsMessage}");
                TolkHelper.Speak(ttsMessage);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"ReadShopItem Error: {ex.Message}");
        }
    }

    // --- Read current balance (Alt+M hotkey) ---
    public static void ReadCurrentMoney()
    {
        try
        {
            MelonLogger.Msg("[Shop-TTS] ReadCurrentMoney called");
            MenuShop shop = UnityEngine.Object.FindObjectOfType<MenuShop>();
            if (shop != null && shop.gameObject.activeInHierarchy)
            {
                MelonLogger.Msg("[Shop-TTS] Shop found and active");
                // Field: mMoneyTxt (Text)
                FieldInfo moneyField = AccessTools.Field(typeof(MenuShop), "mMoneyTxt");
                Text moneyText = (Text)moneyField.GetValue(shop);

                if (moneyText != null)
                {
                    string balance = moneyText.text.Trim();
                    MelonLogger.Msg($"[Shop-TTS] Balance: {balance}");
                    TolkHelper.Speak($"Current Balance: {balance}");
                }
                else
                {
                    MelonLogger.Msg("[Shop-TTS] moneyText is null");
                    TolkHelper.Speak("Could not read balance.");
                }
            }
            else
            {
                MelonLogger.Msg("[Shop-TTS] Shop not found or not active");
                TolkHelper.Speak("Shop not active.");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[Shop-TTS] ReadCurrentMoney error: {ex.Message}");
        }
    }
}