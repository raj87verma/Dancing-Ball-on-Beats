# Music Credits — Dancing Ball on Beats

This app bundles three real audio tracks (Option A) plus three fully
procedural, on-device-generated tracks (Option C). No copyrighted or
licensed commercial music (e.g. from the original Marble Music / Beat Bounce
apps) is used anywhere in this project.

## Bundled real tracks (Public Domain / CC0 1.0)

All three tracks below were sourced from the FreePD public-domain music
archive and are released under **CC0 1.0 Universal (Public Domain
Dedication)** — free to use, modify, and redistribute, including for
commercial purposes, with **no attribution legally required**. Credits are
listed here and shown in-app as a courtesy.

| File | Track Title | Source |
|---|---|---|
| `Backbeat.mp3` | Backbeat | freepd.com (mirror: github.com/0lhi/FreePD) |
| `BitBitLoop.mp3` | Bit Bit Loop | freepd.com (mirror: github.com/0lhi/FreePD) |
| `HippetyHop.mp3` | Hippety Hop | freepd.com (mirror: github.com/0lhi/FreePD) |

License text (CC0 1.0 Universal): https://creativecommons.org/publicdomain/zero/1.0/

Original archive: freepd.com (site has since closed; a full historical
mirror of its CC0-licensed catalog is preserved at
https://github.com/0lhi/FreePD under the same CC0 terms).

**Before shipping to production**, re-verify these files still carry CC0
terms at the mirror above (or replace with any other CC0/public-domain
track you prefer, e.g. from https://pixabay.com/music/ filtering by "CC0" /
"no attribution required", or your own compositions) and update this file
and `AudioTrackLibrary.cs` accordingly.

## Procedural tracks (100% original, generated at runtime)

"Pixel Dash", "Calm Orbit", and "Neon Rush" are not audio files at all —
they are synthesized live on the player's device by `ChiptuneSynth.cs` and
`ProceduralBeatComposer.cs` using simple waveform generation (square,
triangle, sine, sawtooth) and a seeded note-pattern algorithm. There is
zero licensing risk for these tracks since no external audio is used.

## Adding your own music later

To add a new bundled track:
1. Drop a CC0/public-domain (or a track you personally hold full rights to)
   `.mp3`/`.wav` file into `Assets/Resources/Songs/`.
2. Add an entry to `AudioTrackLibrary.BuildDefaultLibrary()` with the
   correct `resourceClipPath` (filename without extension) and BPM.
3. Add a row to the table above with the license source.
