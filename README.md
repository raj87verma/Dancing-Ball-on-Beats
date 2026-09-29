# Dancing Ball on Beats

A free, original rhythm-runner game for Android: guide a bouncing marble along a
beat-synced track, switching lanes and jumping in time with the music. Inspired by
the *gameplay concept* of apps like "Marble Music – Beat Bounce" — **no code,
art, audio, or branding from those apps is used anywhere in this project.**

## What's in this project

- **Unity version:** 6000.0.36f1 (Unity 6 LTS). Open with Unity Hub — it will
  offer to install this exact version if you don't have it, or you can use any
  nearby 6000.0.x LTS release (the project has no exotic dependencies).
- **Package name:** `com.dancingballstudio.dancingballonbeats` (change this in
  `Project Settings > Player > Android > Other Settings > Identification`
  before you publish — it must be unique on the Play Store).
- **Everything is built at runtime in C#** — there are no hand-authored
  prefabs or 3D models. Scenes (`MainMenu.unity`, `Gameplay.unity`) each
  contain a single bootstrap `MonoBehaviour` that constructs the entire
  scene (camera, lighting, UI, ball, track, managers) in `Awake()`. This
  keeps the project 100% readable in a text editor and avoids any risk of
  corrupt/binary scene files.

## How to open and run it

1. Install **Unity Hub** (https://unity.com/download) if you don't have it.
2. In Unity Hub, click **Add project from disk** and select this
   `DancingBallOnBeats` folder. Hub will prompt to install Editor version
   `6000.0.36f1` if missing — accept, or pick your closest installed 6000.0.x LTS.
3. Open the project. Unity will import assets on first open (may take a
   minute or two).
4. In the **Project** window, open `Assets/Scenes/MainMenu.unity` and press
   **Play** in the Editor to test on desktop (keyboard: ←/→ to change lane,
   Space/↑ or a mouse click to jump).
5. On a phone/tablet, use touch: swipe left/right to change lane, swipe
   up (or tap) to jump.

## Building for Android

1. `File > Build Settings > Android`, click **Switch Platform**.
2. Make sure both scenes are listed under **Scenes In Build** in this order
   (already configured in `ProjectSettings/EditorBuildSettings.asset`):
   `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/Gameplay.unity`.
3. `Player Settings`:
   - Set your own unique **Package Name**.
   - Set **Minimum API Level** to Android 8.0 (API 26) or above (already
     configured).
   - Add an app icon (`Player Settings > Icon`) — none is bundled yet, see
     "Next steps" below.
4. Connect a device (with USB debugging enabled) or click **Build** to
   produce an APK/AAB.

## Music: how the two "free" options work

You asked for **both** a bundled royalty-free library (Option A) and fully
procedural, on-device-generated music (Option C) — both are implemented and
selectable from the Song Select screen:

### Option A — bundled CC0 (public domain) tracks
Three real audio tracks live in `Assets/Resources/Songs/`:
`Backbeat.mp3`, `BitBitLoop.mp3`, `HippetyHop.mp3`. They were sourced from the
FreePD public-domain archive (mirrored at
[github.com/0lhi/FreePD](https://github.com/0lhi/FreePD)) and are licensed
**CC0 1.0 Universal** — free for any use, including commercial, with no
attribution legally required. Full details and how to add more tracks are in
[`Assets/Resources/Songs/Credits/CREDITS.md`](Assets/Resources/Songs/Credits/CREDITS.md).

**Before you publish**, please re-verify the source's license status
yourself (the original freepd.com site has closed down; only the GitHub
mirror was checked here) and consider swapping in tracks you personally
like from other CC0 sources such as
[pixabay.com/music](https://pixabay.com/music/) (filter by "CC0"/no
attribution).

### Option C — 100% procedural / generated music
Three more songs ("Pixel Dash", "Calm Orbit", "Neon Rush") have **no audio
file at all**. `ChiptuneSynth.cs` synthesizes waveforms (square/triangle/
sine/sawtooth) directly into `AudioClip`s at runtime, and
`ProceduralBeatComposer.cs` generates a seeded, repeatable note pattern
(different scale/rhythm per style) that `BeatManager.cs` schedules and plays
bar-by-bar. Zero licensing risk, and you can add more procedural "songs" for
free just by adding a new `SongData` entry with a new seed/BPM/style — see
`AudioTrackLibrary.cs`.

## Project structure

```
Assets/
  Scenes/
    MainMenu.unity        Boots MainMenuController (home / song select / settings)
    Gameplay.unity         Boots GameplayBootstrap (assembles a full run)
  Scripts/
    Bootstrap/            Scene entry points (MainMenuBootstrap, GameplayBootstrap)
    Core/                 GameManager (score/lives/combo/game-over), GameSession
    Data/                 SongData, LevelData, SaveSystem (JSON save file)
    Audio/                BeatManager, ChiptuneSynth, ProceduralBeatComposer,
                           AudioTrackLibrary (song catalog)
    Gameplay/              BallController, ObstacleSpawner, Obstacle, LaneTrack,
                           ComboSystem, CameraFollow
    UI/                    UIFactory (runtime uGUI helpers), HUDController,
                           MainMenuController, SongSelectController,
                           SettingsController
    Ads/                    AdsManager (no-op stub, see below)
  Resources/Songs/         Bundled CC0 mp3s + CREDITS.md
```

## Gameplay overview (v1 scope)

- 6 selectable songs (3 real + 3 procedural), each auto-mapped to a
  difficulty-appropriate obstacle pattern (`Classic`, `ZigZag`, `GapRun`,
  `Chaos`) and lane count (3–4 lanes) — see `GameplayBootstrap.BuildLevelForSong`.
- Swipe/tap controls: change lane, jump over gaps, timed to the beat.
- Hits are graded **Perfect / Good / Miss** based on proximity to the beat
  (`BallController.EvaluateTiming`), feeding a combo multiplier
  (`ComboSystem`) that scales score.
- 3 lives per run; losing all lives ends the run and saves your high score
  locally (`SaveSystem`, JSON file in `Application.persistentDataPath`,
  with a `PlayerPrefs` fallback).
- Pause/resume, retry, and quit-to-menu are all wired in the HUD.

## Monetization

This build ships **ad-free** by default (`AdsManager.AdsEnabled = false`).
`AdsManager.cs` is a fully-formed no-op stub — every method (`ShowBanner`,
`ShowInterstitial`, `ShowRewarded`, etc.) already exists and is called from
sensible places conceptually (e.g. you could call `ShowRewarded` from a
"watch ad for an extra life" button on the Game Over screen). To turn on
real ads later:
1. Install the Google Mobile Ads Unity plugin.
2. Replace the method bodies in `AdsManager.cs` with real SDK calls.
3. Flip `AdsEnabled = true`.

## What's NOT included yet (manual steps before Play Store submission)

This sandbox has no Unity Editor GUI, so the following need a real machine
with Unity installed:

- **App icon & splash screen** — no icon is bundled; add one via
  `Player Settings > Icon` (needs a 512x512 PNG at minimum) and adaptive
  icon layers for Android.
- **Signing / keystore** — you'll need to create your own Android keystore
  for release builds (`Player Settings > Publishing Settings`).
- **Store listing assets** — screenshots, feature graphic, short/long
  description, privacy policy URL (required by Play Console) — none of this
  is part of the codebase.
- **First actual compile/Play-mode test** — I could not run the Unity
  Editor in this sandbox, so while every script was written carefully and
  cross-checked for consistent namespaces/references, please open the
  project in the Editor and check the Console for any compile errors before
  building — this is standard practice for any newly scaffolded Unity
  project and something only the Editor's own compiler can fully verify.
- **BPM tuning for the 3 real tracks** — the BPM values in
  `AudioTrackLibrary.cs` (120/128/110) are reasonable estimates; listen and
  nudge `SongData.bpm` in the Inspector if the visual "pulse" feels
  slightly off the actual beat.
- **Balancing** — starting lives, scroll speed, ramp rate, and beat timing
  windows are first-pass values in `LevelData`/`BallController`; playtest
  and tune to taste.

## License note (project code)

All C# code here is original, written for this project. The bundled `.mp3`
files are public domain (CC0) per the credits file. No assets, code, or
copyrighted material from Marble Music, Beat Bounce, or any other
commercial app are included.
