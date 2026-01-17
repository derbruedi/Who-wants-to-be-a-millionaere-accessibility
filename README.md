# WWTBAM TTS Accessibility Mod

A MelonLoader mod that adds Text-to-Speech (TTS) accessibility support for "Who Wants to Be a Millionaire? - New Edition" using screen readers like NVDA, JAWS, or Window-Eyes via the Tolk library.

## Requirements

- [MelonLoader](https://melonwiki.xyz/) installed for the game
- A compatible screen reader (NVDA, JAWS, Window-Eyes, etc.)
- `Tolk.dll` placed in the game folder

## Installation

1. Install MelonLoader v0.7.1 for the game
2. Copy `Tolk.dll` to the game's root folder
3. Copy the compiled mod DLL to the `Mods` folder

---

## Keyboard Shortcuts

### TTS Mod Hotkeys

| Key | Function |
|-----|----------|
| **R** | Repeat the current question |
| **M** | Announce current prize level ("Playing for: X") |
| **W** | Announce winnings so far ("Won: X") |
| **P** | Read audience poll results (after using Ask the Audience) |
| **I** | Read candidate biography |
| **O** | Read online lobby status (players, timer) |
| **Alt+M** | Read current balance in the shop |

### Base Game Controls (Important!)

These are built-in game controls that are essential for gameplay but not obvious to blind players:

#### Accessing Lifelines

| Key/Button | Function |
|------------|----------|
| **Shift** (keyboard) | Access lifeline selection |
| **LB/RB** (controller bumpers) | Access lifeline selection |

#### General Navigation

| Key/Button | Function |
|------------|----------|
| **Arrow Keys** | Navigate menus |
| **Enter / Space** | Select / Confirm |
| **Escape / Backspace** | Back / Cancel |
| **Start** (controller) | Pause game |

#### Answering Questions

| Key/Button | Function |
|------------|----------|
| **Arrow Keys** | Move between answers A/B/C/D |
| **Enter** | Lock in answer |
| **1-4** or **A/B/C/D** | Quick select answer (if enabled) |

#### Controller Layout

| Button | Function |
|--------|----------|
| **A** | Select / Confirm |
| **B** | Back / Cancel |
| **D-Pad** | Navigate |
| **LB / RB** | Access lifelines |
| **Start** | Pause |

---

## What Gets Read Automatically

### Menus
- Main menu navigation
- Options menu (Audio, Video, Language settings)
- Shop items and prices
- Pack selection (with purchase status)
- Character/candidate selection

### Gameplay
- New questions when they appear
- Answer options when focused
- Lifeline selection
- Current money level changes
- Pause menu options

### First Launch
- Language selection screen
- Timer enable/disable screen

---

## Troubleshooting

### TTS Not Working
1. Make sure `Tolk.dll` is in the game folder
2. Ensure your screen reader is running before starting the game
3. Check MelonLoader console for error messages

### Specific Screens Not Reading
- Some screens may require specific navigation to trigger TTS
- Use the hotkeys (R, M, W, P, I, O) to manually request information

---

## Credits

- Mod development: Assisted by Gemini and Claude
- Tolk Library: Screen reader abstraction layer
- MelonLoader: Unity mod loading framework
- HarmonyLib: Runtime patching library

---

## Feedback

If you encounter any issues or have suggestions for improvement, please report them!
