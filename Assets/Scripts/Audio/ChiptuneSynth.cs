using System;
using UnityEngine;

namespace DancingBallOnBeats.Audio
{
    /// <summary>Basic waveform shapes we can synthesize without any audio asset.</summary>
    public enum WaveShape
    {
        Square,
        Triangle,
        Sine,
        Sawtooth
    }

    /// <summary>
    /// Generates short note AudioClips entirely in code (no bundled samples), using classic
    /// chiptune-style waveforms. This is the synthesis backbone for Option C (procedural music):
    /// no royalty/licensing concerns at all since every sample is generated on-device at runtime.
    /// Clips are cached per (frequency, duration, wave, sampleRate) so repeated notes are cheap.
    /// </summary>
    public class ChiptuneSynth : MonoBehaviour
    {
        [Header("Synth Defaults")]
        [SerializeField] private int sampleRate = 44100;
        [SerializeField] private float noteDurationSeconds = 0.18f;
        [SerializeField] [Range(0f, 1f)] private float amplitude = 0.35f;
        [SerializeField] [Range(0f, 0.5f)] private float attackSeconds = 0.005f;
        [SerializeField] [Range(0f, 0.5f)] private float releaseSeconds = 0.08f;

        private AudioSource _source;
        private readonly System.Collections.Generic.Dictionary<string, AudioClip> _clipCache =
            new System.Collections.Generic.Dictionary<string, AudioClip>();

        public float NoteDurationSeconds => noteDurationSeconds;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = false;
            _source.spatialBlend = 0f;
        }

        /// <summary>Standard equal-tempered frequency for a MIDI note number (69 = A4 = 440Hz).</summary>
        public static float MidiToFrequency(int midiNote)
        {
            return 440f * Mathf.Pow(2f, (midiNote - 69) / 12f);
        }

        public void PlayNote(int midiNote, WaveShape wave, float volumeScale = 1f)
        {
            float freq = MidiToFrequency(midiNote);
            AudioClip clip = GetOrCreateClip(freq, wave);
            _source.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        public void PlayFrequency(float frequencyHz, WaveShape wave, float volumeScale = 1f)
        {
            AudioClip clip = GetOrCreateClip(frequencyHz, wave);
            _source.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        private AudioClip GetOrCreateClip(float frequencyHz, WaveShape wave)
        {
            string key = $"{wave}_{frequencyHz:F2}_{noteDurationSeconds:F3}";
            if (_clipCache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            AudioClip clip = GenerateToneClip(frequencyHz, noteDurationSeconds, wave, sampleRate, amplitude, attackSeconds, releaseSeconds);
            _clipCache[key] = clip;
            return clip;
        }

        /// <summary>
        /// Builds a mono PCM AudioClip in memory for a single synthesized note, with a short
        /// linear attack/release envelope so notes don't click.
        /// </summary>
        public static AudioClip GenerateToneClip(
            float frequencyHz,
            float durationSeconds,
            WaveShape wave,
            int sampleRate,
            float amplitude,
            float attackSeconds,
            float releaseSeconds)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(durationSeconds * sampleRate));
            var samples = new float[sampleCount];

            int attackSamples = Mathf.Clamp(Mathf.RoundToInt(attackSeconds * sampleRate), 0, sampleCount / 2);
            int releaseSamples = Mathf.Clamp(Mathf.RoundToInt(releaseSeconds * sampleRate), 0, sampleCount / 2);

            double phase = 0.0;
            double phaseIncrement = frequencyHz / sampleRate;

            for (int i = 0; i < sampleCount; i++)
            {
                float raw = EvaluateWave(wave, phase);
                phase += phaseIncrement;
                if (phase >= 1.0) phase -= 1.0;

                float env = 1f;
                if (i < attackSamples && attackSamples > 0)
                    env = i / (float)attackSamples;
                else if (i >= sampleCount - releaseSamples && releaseSamples > 0)
                    env = (sampleCount - i) / (float)releaseSamples;

                samples[i] = raw * amplitude * env;
            }

            AudioClip clip = AudioClip.Create(
                $"synth_{wave}_{frequencyHz:F0}hz",
                sampleCount,
                1,
                sampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float EvaluateWave(WaveShape wave, double phase01)
        {
            switch (wave)
            {
                case WaveShape.Sine:
                    return Mathf.Sin((float)(phase01 * Math.PI * 2.0));

                case WaveShape.Square:
                    return phase01 < 0.5 ? 1f : -1f;

                case WaveShape.Triangle:
                {
                    // Triangle wave: linear ramp -1 -> 1 -> -1 across one cycle.
                    double t = phase01;
                    double folded = t - Math.Floor(t + 0.5);
                    return (float)(2.0 * Math.Abs(2.0 * folded) - 1.0);
                }

                case WaveShape.Sawtooth:
                    return (float)(2.0 * (phase01 - Math.Floor(phase01 + 0.5)));

                default:
                    return 0f;
            }
        }
    }
}
