using System;
using UnityEngine;

namespace DancingBallOnBeats.Data
{
    /// <summary>
    /// Where a song's audio comes from.
    /// RealTrack  = a bundled CC0 / public-domain audio clip (Option A).
    /// Procedural = generated at runtime by the ProceduralBeatComposer, no audio file needed (Option C).
    /// </summary>
    public enum SongSourceMode
    {
        RealTrack,
        Procedural
    }

    /// <summary>
    /// Describes a single playable song/track: metadata, tempo and where to load its audio from.
    /// Create instances via Assets > Create > DancingBallOnBeats > Song Data,
    /// or build them at runtime (see AudioTrackLibrary.BuildDefaultLibrary).
    /// </summary>
    [CreateAssetMenu(fileName = "NewSongData", menuName = "DancingBallOnBeats/Song Data", order = 1)]
    [Serializable]
    public class SongData : ScriptableObject
    {
        [Header("Identity")]
        public string songId;
        public string displayName;
        public string artistCredit;
        [TextArea(2, 4)]
        public string licenseInfo;

        [Header("Playback")]
        public SongSourceMode sourceMode = SongSourceMode.RealTrack;
        [Tooltip("Path under Assets/Resources, without extension, e.g. 'Songs/Backbeat'.")]
        public string resourceClipPath;
        [Tooltip("Beats per minute. Required for both real tracks and procedural generation.")]
        public float bpm = 120f;
        [Tooltip("Seconds before the first beat lands (for real tracks with lead-in silence).")]
        public float offsetSeconds = 0f;
        [Tooltip("Approximate duration in seconds. Used for procedural tracks and UI display.")]
        public float durationSeconds = 60f;

        [Header("Procedural Settings (used only if sourceMode == Procedural)")]
        public int proceduralSeed = 0;
        public ProceduralStyle proceduralStyle = ProceduralStyle.Chiptune8Bit;

        [Header("Difficulty / Presentation")]
        [Range(1, 5)] public int difficulty = 1;
        public Color themeColor = new Color(0.2f, 0.8f, 1f);

        /// <summary>Convenience factory for building SongData purely in code (no .asset file needed).</summary>
        public static SongData CreateRuntime(
            string id,
            string name,
            string artist,
            string license,
            SongSourceMode mode,
            string clipPath,
            float bpm,
            float durationSeconds,
            int difficulty,
            Color color,
            int proceduralSeed = 0,
            ProceduralStyle style = ProceduralStyle.Chiptune8Bit)
        {
            var song = CreateInstance<SongData>();
            song.songId = id;
            song.displayName = name;
            song.artistCredit = artist;
            song.licenseInfo = license;
            song.sourceMode = mode;
            song.resourceClipPath = clipPath;
            song.bpm = bpm;
            song.durationSeconds = durationSeconds;
            song.difficulty = Mathf.Clamp(difficulty, 1, 5);
            song.themeColor = color;
            song.proceduralSeed = proceduralSeed;
            song.proceduralStyle = style;
            return song;
        }
    }

    /// <summary>Musical flavor used by the procedural composer to pick scales/instruments.</summary>
    public enum ProceduralStyle
    {
        Chiptune8Bit,
        Ambient,
        EDMPulse
    }
}
