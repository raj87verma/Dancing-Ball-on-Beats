using System;
using UnityEngine;
using DancingBallOnBeats.Audio;
using DancingBallOnBeats.Data;
using DancingBallOnBeats.Gameplay;

namespace DancingBallOnBeats.Core
{
    /// <summary>
    /// The central orchestrator for a single gameplay run: wires the ball, spawner, and beat
    /// clock together, tracks score/lives/combo, and raises high-level events that the UI layer
    /// listens to. Deliberately owns no rendering/UI code itself — GameplayBootstrap constructs
    /// the scene and hands this manager the pieces it needs via Initialize().
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<float, float> OnScoreChanged;      // (newScore, delta)
        public event Action<int> OnLivesChanged;
        public event Action<int, float> OnComboChanged;        // (combo, multiplier)
        public event Action<HitJudgement> OnJudgement;
        public event Action OnGameOver;
        public event Action OnGameStarted;
        public event Action OnPaused;
        public event Action OnResumed;

        public float Score { get; private set; }
        public int Lives { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool IsPaused { get; private set; }
        public SongData CurrentSong { get; private set; }
        public LevelData CurrentLevel { get; private set; }

        private ComboSystem _comboSystem;
        private BallController _ballController;
        private ObstacleSpawner _obstacleSpawner;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(SongData song, LevelData level, BallController ballController, ObstacleSpawner obstacleSpawner)
        {
            CurrentSong = song;
            CurrentLevel = level;
            _ballController = ballController;
            _obstacleSpawner = obstacleSpawner;

            Score = 0f;
            Lives = level.startingLives;
            IsGameOver = false;
            IsPaused = false;

            _comboSystem = new ComboSystem(level.comboMultiplierStep);
            _comboSystem.OnComboChanged += combo => OnComboChanged?.Invoke(combo, _comboSystem.CurrentMultiplier);
            _comboSystem.OnComboBroken += () => OnComboChanged?.Invoke(0, 1f);

            _ballController.OnObstacleResolved += HandleObstacleResolved;

            var savedVolumes = SaveSystem.Current;
            if (BeatManager.Instance != null)
            {
                BeatManager.Instance.SetMusicVolume(savedVolumes.musicMuted ? 0f : savedVolumes.musicVolume);
                BeatManager.Instance.OnSongEnded += HandleSongEnded;
            }
        }

        public void StartRun()
        {
            if (BeatManager.Instance != null && CurrentSong != null)
            {
                BeatManager.Instance.PlaySong(CurrentSong);
            }
            OnGameStarted?.Invoke();
        }

        private void HandleObstacleResolved(HitJudgement judgement, Obstacle obstacle)
        {
            if (IsGameOver) return;

            OnJudgement?.Invoke(judgement);

            if (judgement == HitJudgement.Miss)
            {
                LoseLife();
                return;
            }

            float baseScore = CurrentLevel != null ? CurrentLevel.scorePerBeatHit : 10f;
            float bonusScale = obstacle.Type == ObstacleType.BonusNote ? 2f : 1f;
            float awarded = _comboSystem.RegisterHit(judgement, baseScore * bonusScale);

            Score += awarded;
            OnScoreChanged?.Invoke(Score, awarded);
        }

        private void LoseLife()
        {
            Lives = Mathf.Max(0, Lives - 1);
            OnLivesChanged?.Invoke(Lives);

            if (Lives <= 0)
            {
                EndGame();
            }
        }

        private void HandleSongEnded()
        {
            if (!IsGameOver)
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            if (IsGameOver) return;
            IsGameOver = true;

            if (_ballController != null) _ballController.InputEnabled = false;
            if (BeatManager.Instance != null) BeatManager.Instance.StopSong();

            if (CurrentSong != null)
            {
                SaveSystem.SubmitScore(CurrentSong.songId, Mathf.RoundToInt(Score), _comboSystem.BestCombo);
            }

            OnGameOver?.Invoke();
        }

        public void TogglePause()
        {
            if (IsGameOver) return;

            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            if (_ballController != null) _ballController.InputEnabled = !IsPaused;

            if (IsPaused) OnPaused?.Invoke();
            else OnResumed?.Invoke();
        }

        public void RestartRun()
        {
            Time.timeScale = 1f;
            // Reloading the active scene is the simplest, most robust "restart" for this scope —
            // GameplayBootstrap will rebuild everything (ball, spawner, beat manager) fresh.
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.SceneManager.LoadScene(activeScene.name);
        }

        public void QuitToMenu()
        {
            Time.timeScale = 1f;
            if (BeatManager.Instance != null) BeatManager.Instance.StopSong();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (_ballController != null) _ballController.OnObstacleResolved -= HandleObstacleResolved;
            if (BeatManager.Instance != null) BeatManager.Instance.OnSongEnded -= HandleSongEnded;
        }
    }
}
