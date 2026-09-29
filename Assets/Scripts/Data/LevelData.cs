using System;
using UnityEngine;

namespace DancingBallOnBeats.Data
{
    /// <summary>
    /// Which obstacle pattern generator to use for a run. Patterns are generated procedurally
    /// from the song's BPM/seed rather than hand-authored, so every song automatically has a level.
    /// </summary>
    public enum ObstaclePattern
    {
        Classic,     // simple alternating lane blocks
        ZigZag,      // forces regular lane switches
        GapRun,      // periodic full-width gaps requiring jump
        Chaos        // mixes all pattern types, scales with difficulty
    }

    /// <summary>
    /// Runtime description of a playable level: which song drives it, how many lanes,
    /// how obstacles are laid out, and scoring/lives tuning. Built in code at app start
    /// (see LevelLibrary) — no hand-authored .asset files required.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "DancingBallOnBeats/Level Data", order = 2)]
    [Serializable]
    public class LevelData : ScriptableObject
    {
        [Header("Song Link")]
        public string songId;

        [Header("Track Layout")]
        [Range(2, 5)] public int laneCount = 3;
        public ObstaclePattern pattern = ObstaclePattern.Classic;
        [Tooltip("How many beats between obstacle spawns. Lower = denser.")]
        public int beatsPerObstacle = 1;

        [Header("Rules")]
        public int startingLives = 3;
        public float scorePerBeatHit = 10f;
        public float comboMultiplierStep = 0.1f;
        [Tooltip("Speed the obstacle track scrolls toward the player, in units/sec.")]
        public float baseScrollSpeed = 6f;
        [Tooltip("Scroll speed increases by this amount every N beats to ramp difficulty.")]
        public float speedRampPerBeat = 0.01f;

        public static LevelData CreateRuntime(
            string songId,
            int laneCount,
            ObstaclePattern pattern,
            int beatsPerObstacle,
            int startingLives,
            float baseScrollSpeed)
        {
            var level = CreateInstance<LevelData>();
            level.songId = songId;
            level.laneCount = Mathf.Clamp(laneCount, 2, 5);
            level.pattern = pattern;
            level.beatsPerObstacle = Mathf.Max(1, beatsPerObstacle);
            level.startingLives = Mathf.Max(1, startingLives);
            level.baseScrollSpeed = baseScrollSpeed;
            return level;
        }
    }
}
