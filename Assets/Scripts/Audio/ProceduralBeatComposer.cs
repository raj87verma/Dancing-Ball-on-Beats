using System;
using System.Collections.Generic;
using UnityEngine;
using DancingBallOnBeats.Data;

namespace DancingBallOnBeats.Audio
{
    /// <summary>
    /// One scheduled note event within a generated bar.
    /// </summary>
    public struct ProceduralNoteEvent
    {
        public float beatOffset;   // offset from bar start, in beats (e.g. 0, 0.5, 1, 1.5...)
        public int midiNote;
        public WaveShape wave;
        public float volume;
        public bool isDrum;        // drums use a fixed low "click" tone instead of scale note
    }

    /// <summary>
    /// Generates deterministic (seeded) procedural music patterns with no bundled audio —
    /// this is Option C. Given a SongData in Procedural mode, it produces a repeating bar of
    /// notes driven by the song's BPM and style, which BeatManager plays back in sync with
    /// gameplay beats using ChiptuneSynth. Because it is seeded, the same song always sounds
    /// the same across a play session (and is easy to keep in sync with obstacle patterns).
    /// </summary>
    public static class ProceduralBeatComposer
    {
        // Simple diatonic minor pentatonic-ish scale intervals (in semitones from root) —
        // sounds pleasant with random note choice, avoiding dissonant combinations.
        private static readonly int[] PentatonicMinor = { 0, 3, 5, 7, 10, 12 };
        private static readonly int[] MajorScale = { 0, 2, 4, 5, 7, 9, 11, 12 };

        /// <summary>
        /// Builds one bar (4 beats) of note events for the given style/seed.
        /// Call again with an incrementing barIndex to get variation across a full song,
        /// while staying deterministic for a given (seed, barIndex) pair.
        /// </summary>
        public static List<ProceduralNoteEvent> GenerateBar(ProceduralStyle style, int seed, int barIndex, int rootMidiNote = 57)
        {
            var rng = new System.Random(seed * 7919 + barIndex * 104729);
            var events = new List<ProceduralNoteEvent>();

            switch (style)
            {
                case ProceduralStyle.Chiptune8Bit:
                    BuildChiptuneBar(events, rng, rootMidiNote);
                    break;
                case ProceduralStyle.Ambient:
                    BuildAmbientBar(events, rng, rootMidiNote);
                    break;
                case ProceduralStyle.EDMPulse:
                    BuildEdmBar(events, rng, rootMidiNote);
                    break;
            }

            return events;
        }

        private static void BuildChiptuneBar(List<ProceduralNoteEvent> events, System.Random rng, int root)
        {
            // Melody on every beat (4 notes), plus a drum click on beats 0 and 2 (kick feel).
            for (int beat = 0; beat < 4; beat++)
            {
                int degree = PentatonicMinor[rng.Next(PentatonicMinor.Length)];
                int octaveJitter = rng.Next(0, 2) * 12;
                events.Add(new ProceduralNoteEvent
                {
                    beatOffset = beat,
                    midiNote = root + degree + octaveJitter,
                    wave = WaveShape.Square,
                    volume = 0.8f,
                    isDrum = false
                });
            }

            events.Add(new ProceduralNoteEvent { beatOffset = 0f, midiNote = root - 24, wave = WaveShape.Triangle, volume = 1f, isDrum = true });
            events.Add(new ProceduralNoteEvent { beatOffset = 2f, midiNote = root - 24, wave = WaveShape.Triangle, volume = 0.9f, isDrum = true });
        }

        private static void BuildAmbientBar(List<ProceduralNoteEvent> events, System.Random rng, int root)
        {
            // Sparser, longer sustained-feel notes using Sine, only on beats 0 and 2.5 for a calm ASMR feel.
            int degree1 = MajorScale[rng.Next(MajorScale.Length)];
            int degree2 = MajorScale[rng.Next(MajorScale.Length)];

            events.Add(new ProceduralNoteEvent { beatOffset = 0f, midiNote = root + degree1, wave = WaveShape.Sine, volume = 0.6f, isDrum = false });
            events.Add(new ProceduralNoteEvent { beatOffset = 2.5f, midiNote = root + degree2 + 12, wave = WaveShape.Sine, volume = 0.5f, isDrum = false });
        }

        private static void BuildEdmBar(List<ProceduralNoteEvent> events, System.Random rng, int root)
        {
            // Dense 8th-note pulse with sawtooth lead and a kick on every beat — energetic EDM feel.
            for (int i = 0; i < 8; i++)
            {
                float beatOffset = i * 0.5f;
                int degree = PentatonicMinor[rng.Next(PentatonicMinor.Length)];
                events.Add(new ProceduralNoteEvent
                {
                    beatOffset = beatOffset,
                    midiNote = root + degree + 12,
                    wave = WaveShape.Sawtooth,
                    volume = 0.55f,
                    isDrum = false
                });
            }
            for (int beat = 0; beat < 4; beat++)
            {
                events.Add(new ProceduralNoteEvent { beatOffset = beat, midiNote = root - 24, wave = WaveShape.Square, volume = 1f, isDrum = true });
            }
        }
    }
}
