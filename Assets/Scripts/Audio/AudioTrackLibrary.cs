using System.Collections.Generic;
using UnityEngine;
using DancingBallOnBeats.Data;

namespace DancingBallOnBeats.Audio
{
    /// <summary>
    /// Central catalog of every playable song in the game: both the bundled CC0/public-domain
    /// tracks (Option A) and the built-in procedural songs (Option C). Built entirely in code
    /// so no manual ScriptableObject asset wiring is required in the Editor — just instantiate
    /// AudioTrackLibrary.BuildDefaultLibrary() at boot.
    ///
    /// IMPORTANT (licensing): the three bundled mp3s under Assets/Resources/Songs/ were sourced
    /// from the FreePD public-domain music archive (https://freepd.com, mirrored at
    /// https://github.com/0lhi/FreePD) and are released under CC0 1.0 (public domain) — free for
    /// commercial use with no attribution required. Credits are still listed in
    /// Assets/Resources/Songs/Credits/CREDITS.md as a courtesy and are shown in-app on the
    /// Settings screen. Do NOT add any other copyrighted/licensed music without updating this file
    /// and the credits doc, and confirming license terms.
    /// </summary>
    public static class AudioTrackLibrary
    {
        public static List<SongData> BuildDefaultLibrary()
        {
            var list = new List<SongData>();

            // ---- Option A: bundled CC0 / public-domain real tracks ----
            // BPM values below are reasonable working estimates for these tracks; fine-tune by
            // ear in the Editor (SongData.bpm) if the beat pulse feels slightly off once you
            // hear it against gameplay - this does not require re-importing the audio.
            list.Add(SongData.CreateRuntime(
                id: "real_backbeat",
                name: "Backbeat",
                artist: "Kevin MacLeod / FreePD contributors",
                license: "Public Domain (CC0 1.0) - freepd.com",
                mode: SongSourceMode.RealTrack,
                clipPath: "Songs/Backbeat",
                bpm: 120f,
                durationSeconds: 46f,
                difficulty: 1,
                color: new Color(0.25f, 0.85f, 0.95f)));

            list.Add(SongData.CreateRuntime(
                id: "real_bitbitloop",
                name: "Bit Bit Loop",
                artist: "Kevin MacLeod / FreePD contributors",
                license: "Public Domain (CC0 1.0) - freepd.com",
                mode: SongSourceMode.RealTrack,
                clipPath: "Songs/BitBitLoop",
                bpm: 128f,
                durationSeconds: 77f,
                difficulty: 2,
                color: new Color(0.95f, 0.55f, 0.95f)));

            list.Add(SongData.CreateRuntime(
                id: "real_hippetyhop",
                name: "Hippety Hop",
                artist: "Kevin MacLeod / FreePD contributors",
                license: "Public Domain (CC0 1.0) - freepd.com",
                mode: SongSourceMode.RealTrack,
                clipPath: "Songs/HippetyHop",
                bpm: 110f,
                durationSeconds: 117f,
                difficulty: 2,
                color: new Color(1f, 0.75f, 0.3f)));

            // ---- Option C: fully procedural songs (generated on-device, zero licensing risk) ----
            list.Add(SongData.CreateRuntime(
                id: "proc_pixel_dash",
                name: "Pixel Dash (Procedural)",
                artist: "Generated on-device",
                license: "Original procedural composition - no external assets",
                mode: SongSourceMode.Procedural,
                clipPath: null,
                bpm: 132f,
                durationSeconds: 90f,
                difficulty: 2,
                color: new Color(0.4f, 1f, 0.6f),
                proceduralSeed: 1001,
                style: ProceduralStyle.Chiptune8Bit));

            list.Add(SongData.CreateRuntime(
                id: "proc_calm_orbit",
                name: "Calm Orbit (Procedural)",
                artist: "Generated on-device",
                license: "Original procedural composition - no external assets",
                mode: SongSourceMode.Procedural,
                clipPath: null,
                bpm: 90f,
                durationSeconds: 90f,
                difficulty: 1,
                color: new Color(0.6f, 0.75f, 1f),
                proceduralSeed: 2002,
                style: ProceduralStyle.Ambient));

            list.Add(SongData.CreateRuntime(
                id: "proc_neon_rush",
                name: "Neon Rush (Procedural)",
                artist: "Generated on-device",
                license: "Original procedural composition - no external assets",
                mode: SongSourceMode.Procedural,
                clipPath: null,
                bpm: 150f,
                durationSeconds: 90f,
                difficulty: 4,
                color: new Color(1f, 0.3f, 0.4f),
                proceduralSeed: 3003,
                style: ProceduralStyle.EDMPulse));

            return list;
        }

        public static SongData FindById(List<SongData> library, string songId)
        {
            for (int i = 0; i < library.Count; i++)
            {
                if (library[i].songId == songId) return library[i];
            }
            return null;
        }
    }
}
