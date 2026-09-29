using System;
using System.Collections.Generic;
using UnityEngine;
using DancingBallOnBeats.Data;

namespace DancingBallOnBeats.Audio
{
    /// <summary>
    /// The heartbeat of the whole game. Owns the master AudioSource, tracks beat timing for
    /// BOTH playback modes (a bundled CC0 clip, or a fully procedural chiptune track), and
    /// raises OnBeat every time a new beat boundary is crossed so gameplay (obstacle spawner,
    /// scoring window, visual pulse) can react in sync. Uses AudioSettings.dspTime for the
    /// authoritative clock so beat timing does not drift with frame-rate hiccups.
    /// </summary>
    public class BeatManager : MonoBehaviour
    {
        public static BeatManager Instance { get; private set; }

        [Header("Wiring")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private ChiptuneSynth synth;

        public event Action<int> OnBeat;          // fired once per beat, argument = beat index since song start
        public event Action OnSongEnded;

        public float Bpm { get; private set; } = 120f;
        public float SecondsPerBeat => 60f / Mathf.Max(1f, Bpm);
        public bool IsPlaying { get; private set; }
        public int CurrentBeatIndex { get; private set; } = -1;

        /// <summary>Seconds elapsed since song start, using the drift-free DSP clock.</summary>
        public float SongTimeSeconds
        {
            get
            {
                if (!IsPlaying) return 0f;
                return (float)(AudioSettings.dspTime - _dspSongStartTime);
            }
        }

        /// <summary>0..1 progress within the current beat — useful for scoring "how close to the beat" and for pulse visuals.</summary>
        public float BeatProgress01
        {
            get
            {
                float t = SongTimeSeconds - _currentSong.offsetSeconds;
                if (t < 0f) return 0f;
                float mod = t % SecondsPerBeat;
                return mod / SecondsPerBeat;
            }
        }

        private SongData _currentSong;
        private double _dspSongStartTime;
        private int _proceduralBarIndex;
        private double _nextProceduralBarDspTime;
        private const int BeatsPerBar = 4;
        private readonly List<PendingNote> _pendingNotes = new List<PendingNote>();

        private struct PendingNote
        {
            public double dspTime;
            public int midiNote;
            public WaveShape wave;
            public float volume;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = false;

            if (synth == null) synth = gameObject.AddComponent<ChiptuneSynth>();
        }

        private void Update()
        {
            if (!IsPlaying || _currentSong == null) return;

            double dspNow = AudioSettings.dspTime;

            // Detect beat boundary crossings using the DSP clock (frame-rate independent).
            float t = SongTimeSeconds - _currentSong.offsetSeconds;
            if (t >= 0f)
            {
                int beatIndex = Mathf.FloorToInt(t / SecondsPerBeat);
                if (beatIndex > CurrentBeatIndex)
                {
                    CurrentBeatIndex = beatIndex;
                    OnBeat?.Invoke(CurrentBeatIndex);
                }
            }

            if (_currentSong.sourceMode == SongSourceMode.Procedural)
            {
                UpdateProceduralScheduling(dspNow);
            }

            // Fire pending scheduled notes (procedural) when their DSP time arrives.
            for (int i = _pendingNotes.Count - 1; i >= 0; i--)
            {
                if (dspNow >= _pendingNotes[i].dspTime)
                {
                    var n = _pendingNotes[i];
                    synth.PlayFrequency(ChiptuneSynth.MidiToFrequency(n.midiNote), n.wave, n.volume);
                    _pendingNotes.RemoveAt(i);
                }
            }

            if (_currentSong.sourceMode == SongSourceMode.RealTrack && IsPlaying && !musicSource.isPlaying && t > 0.25f)
            {
                IsPlaying = false;
                OnSongEnded?.Invoke();
            }
            else if (_currentSong.sourceMode == SongSourceMode.Procedural && _currentSong.durationSeconds > 0f
                     && t >= _currentSong.durationSeconds)
            {
                IsPlaying = false;
                OnSongEnded?.Invoke();
            }
        }

        /// <summary>Starts playback of a song, real or procedural, resetting the beat clock.</summary>
        public void PlaySong(SongData song)
        {
            StopSong();

            _currentSong = song;
            Bpm = Mathf.Max(1f, song.bpm);
            CurrentBeatIndex = -1;
            _proceduralBarIndex = 0;
            _pendingNotes.Clear();

            if (song.sourceMode == SongSourceMode.RealTrack)
            {
                AudioClip clip = Resources.Load<AudioClip>(song.resourceClipPath);
                if (clip == null)
                {
                    Debug.LogError($"[BeatManager] Could not load clip at Resources/{song.resourceClipPath}. Falling back to procedural silence-free click track.");
                    StartProcedural(song);
                    return;
                }

                musicSource.clip = clip;
                double startDelay = 0.15; // brief lead-in so first Play() call is scheduled cleanly
                _dspSongStartTime = AudioSettings.dspTime + startDelay;
                musicSource.PlayScheduled(_dspSongStartTime);
                IsPlaying = true;
            }
            else
            {
                StartProcedural(song);
            }
        }

        private void StartProcedural(SongData song)
        {
            _dspSongStartTime = AudioSettings.dspTime + 0.1;
            _nextProceduralBarDspTime = _dspSongStartTime;
            IsPlaying = true;
        }

        private void UpdateProceduralScheduling(double dspNow)
        {
            double barDurationSeconds = SecondsPerBeat * BeatsPerBar;

            // Schedule the next bar slightly ahead of time to avoid audio gaps.
            while (_nextProceduralBarDspTime < dspNow + barDurationSeconds)
            {
                var notes = ProceduralBeatComposer.GenerateBar(
                    _currentSong.proceduralStyle,
                    _currentSong.proceduralSeed,
                    _proceduralBarIndex);

                foreach (var note in notes)
                {
                    double noteDspTime = _nextProceduralBarDspTime + note.beatOffset * SecondsPerBeat;
                    _pendingNotes.Add(new PendingNote
                    {
                        dspTime = noteDspTime,
                        midiNote = note.midiNote,
                        wave = note.wave,
                        volume = note.volume
                    });
                }

                _proceduralBarIndex++;
                _nextProceduralBarDspTime += barDurationSeconds;
            }
        }

        public void StopSong()
        {
            if (musicSource.isPlaying) musicSource.Stop();
            IsPlaying = false;
            CurrentBeatIndex = -1;
            _pendingNotes.Clear();
            _currentSong = null;
        }

        public void SetMusicVolume(float volume01)
        {
            musicSource.volume = Mathf.Clamp01(volume01);
        }

        /// <summary>Returns true if the given time (seconds) is within +/- toleranceSeconds of the nearest beat.</summary>
        public bool IsNearBeat(float toleranceSeconds)
        {
            float progress = BeatProgress01;
            float secondsIntoBeat = progress * SecondsPerBeat;
            float secondsUntilNextBeat = SecondsPerBeat - secondsIntoBeat;
            return secondsIntoBeat <= toleranceSeconds || secondsUntilNextBeat <= toleranceSeconds;
        }
    }
}
