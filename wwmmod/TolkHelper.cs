using System.Runtime.InteropServices;
using System.Text;
using MelonLoader;
using System;

public static class TolkHelper
{
        // Name der nativen DLL, die du im Spielordner hast.
        private const string DllName = "Tolk.dll"; 
        
        // ==========================================================
        // 1. P/INVOKE FUNKTIONEN (Import aus Tolk.dll)
        // ==========================================================
        
        [DllImport(DllName, EntryPoint = "Tolk_Load", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Tolk_Load();

        [DllImport(DllName, EntryPoint = "Tolk_Unload", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Tolk_Unload();

        [DllImport(DllName, EntryPoint = "Tolk_Speak", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_Speak([MarshalAs(UnmanagedType.LPWStr)] string text, bool interrupt);

        [DllImport(DllName, EntryPoint = "Tolk_Output", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_Output([MarshalAs(UnmanagedType.LPWStr)] string text, bool interrupt);

        [DllImport(DllName, EntryPoint = "Tolk_IsSpeaking", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_IsSpeaking();

        [DllImport(DllName, EntryPoint = "Tolk_Silence", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_Silence();

        // ==========================================================
        // 2. PUBLIC WRAPPER METHODEN
        // ==========================================================

        /// <summary>
        /// Initialisiert Tolk (sollte in OnInitializeMelon aufgerufen werden).
        /// </summary>
        public static void Load()
        {
            try
            {
                Tolk_Load();
                MelonLogger.Msg("Tolk: Erfolgreich geladen.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Tolk: Fehler beim Laden der DLL. Stelle sicher, dass {DllName} im Spielordner liegt. Fehler: {ex.Message}");
            }
        }

        /// <summary>
        /// De-initialisiert Tolk (sollte in OnDeinitializeMelon aufgerufen werden).
        /// </summary>
        public static void Unload()
        {
            Tolk_Unload();
            MelonLogger.Msg("Tolk: Erfolgreich entladen.");
        }

        /// <summary>
        /// Spricht den gegebenen Text und unterbricht vorherige Sprache.
        /// </summary>
        /// <param name="text">Der zu sprechende Text.</param>
        public static void Speak(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                // Gibt Text sowohl über Sprache als auch Braillezeile aus
                Tolk_Output(text, true);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Tolk: Fehler beim Ausgeben des Textes '{text}'. Fehler: {ex.Message}");
            }
        }

        /// <summary>
        /// Stoppt die aktuelle Sprachausgabe.
        /// </summary>
        public static void Silence()
        {
            try
            {
                Tolk_Silence();
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Tolk: Fehler beim Stoppen der Sprache. Fehler: {ex.Message}");
            }
        }
}