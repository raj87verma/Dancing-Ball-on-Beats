using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DancingBallOnBeats.Core;
using DancingBallOnBeats.Gameplay;

namespace DancingBallOnBeats.UI
{
    /// <summary>
    /// The in-gameplay HUD: score, combo, lives, pause button, and a pause overlay + game-over
    /// overlay. Subscribes to GameManager events and updates purely presentational text/images —
    /// all game rules live in GameManager, this class never mutates gameplay state directly.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _comboText;
        private TextMeshProUGUI _livesText;
        private TextMeshProUGUI _judgementText;
        private GameObject _pauseOverlay;
        private GameObject _gameOverOverlay;
        private TextMeshProUGUI _gameOverScoreText;
        private TextMeshProUGUI _gameOverBestComboText;
        private float _judgementTextTimer;

        public static HUDController Build(Transform canvasRoot)
        {
            var go = new GameObject("HUDController");
            go.transform.SetParent(canvasRoot, false);
            var hud = go.AddComponent<HUDController>();
            hud.BuildUI(canvasRoot);
            return hud;
        }

        private void BuildUI(Transform canvasRoot)
        {
            _scoreText = UIFactory.CreateText(canvasRoot, "ScoreText", "0", 64, Color.white,
                TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(0.6f, 1f),
                new Vector2(30, -140), new Vector2(0, -30));

            _comboText = UIFactory.CreateText(canvasRoot, "ComboText", "", 44, new Color(1f, 0.85f, 0.2f),
                TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(0.6f, 1f),
                new Vector2(30, -200), new Vector2(0, -140));

            _livesText = UIFactory.CreateText(canvasRoot, "LivesText", "❤ ❤ ❤", 48, new Color(1f, 0.3f, 0.35f),
                TextAlignmentOptions.TopRight, new Vector2(0.6f, 1f), new Vector2(1f, 1f),
                new Vector2(0, -140), new Vector2(-30, -30));

            _judgementText = UIFactory.CreateText(canvasRoot, "JudgementText", "", 56, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.3f, 0.55f), new Vector2(0.7f, 0.65f));

            var pauseButton = UIFactory.CreateButton(canvasRoot, "PauseButton", "II",
                new Color(0f, 0f, 0f, 0.4f), Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-110, -130), new Vector2(-30, -30));
            pauseButton.onClick.AddListener(() => GameManager.Instance?.TogglePause());

            BuildPauseOverlay(canvasRoot);
            BuildGameOverOverlay(canvasRoot);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += (score, delta) => _scoreText.text = Mathf.RoundToInt(score).ToString();
                GameManager.Instance.OnComboChanged += (combo, multiplier) =>
                    _comboText.text = combo > 1 ? $"Combo x{combo}  ({multiplier:F1}x)" : "";
                GameManager.Instance.OnLivesChanged += UpdateLives;
                GameManager.Instance.OnJudgement += ShowJudgement;
                GameManager.Instance.OnPaused += () => _pauseOverlay.SetActive(true);
                GameManager.Instance.OnResumed += () => _pauseOverlay.SetActive(false);
                GameManager.Instance.OnGameOver += ShowGameOver;

                UpdateLives(GameManager.Instance.Lives);
            }
        }

        private void BuildPauseOverlay(Transform canvasRoot)
        {
            _pauseOverlay = UIFactory.CreatePanel(canvasRoot, "PauseOverlay", new Color(0f, 0f, 0f, 0.75f), Vector2.zero, Vector2.one);
            _pauseOverlay.SetActive(false);

            UIFactory.CreateText(_pauseOverlay.transform, "PausedLabel", "PAUSED", 72, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.75f));

            var resumeBtn = UIFactory.CreateButton(_pauseOverlay.transform, "ResumeButton", "Resume",
                new Color(0.25f, 0.85f, 0.5f), Color.white,
                new Vector2(0.25f, 0.45f), new Vector2(0.75f, 0.55f));
            resumeBtn.onClick.AddListener(() => GameManager.Instance?.TogglePause());

            var quitBtn = UIFactory.CreateButton(_pauseOverlay.transform, "QuitButton", "Quit to Menu",
                new Color(0.85f, 0.3f, 0.3f), Color.white,
                new Vector2(0.25f, 0.3f), new Vector2(0.75f, 0.4f));
            quitBtn.onClick.AddListener(() => GameManager.Instance?.QuitToMenu());
        }

        private void BuildGameOverOverlay(Transform canvasRoot)
        {
            _gameOverOverlay = UIFactory.CreatePanel(canvasRoot, "GameOverOverlay", new Color(0f, 0f, 0f, 0.85f), Vector2.zero, Vector2.one);
            _gameOverOverlay.SetActive(false);

            UIFactory.CreateText(_gameOverOverlay.transform, "GameOverLabel", "GAME OVER", 76, new Color(1f, 0.3f, 0.35f),
                TextAlignmentOptions.Center, new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.78f));

            _gameOverScoreText = UIFactory.CreateText(_gameOverOverlay.transform, "FinalScoreText", "Score: 0", 52, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.64f));

            _gameOverBestComboText = UIFactory.CreateText(_gameOverOverlay.transform, "BestComboText", "Best Combo: 0", 40, new Color(1f, 0.85f, 0.2f),
                TextAlignmentOptions.Center, new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.55f));

            var retryBtn = UIFactory.CreateButton(_gameOverOverlay.transform, "RetryButton", "Retry",
                new Color(0.25f, 0.85f, 0.5f), Color.white,
                new Vector2(0.25f, 0.32f), new Vector2(0.75f, 0.42f));
            retryBtn.onClick.AddListener(() => GameManager.Instance?.RestartRun());

            var menuBtn = UIFactory.CreateButton(_gameOverOverlay.transform, "MenuButton", "Main Menu",
                new Color(0.3f, 0.5f, 0.9f), Color.white,
                new Vector2(0.25f, 0.19f), new Vector2(0.75f, 0.29f));
            menuBtn.onClick.AddListener(() => GameManager.Instance?.QuitToMenu());
        }

        private void UpdateLives(int lives)
        {
            _livesText.text = string.Concat(System.Linq.Enumerable.Repeat("❤ ", Mathf.Max(0, lives)));
        }

        private void ShowJudgement(HitJudgement judgement)
        {
            switch (judgement)
            {
                case HitJudgement.Perfect:
                    _judgementText.text = "PERFECT!";
                    _judgementText.color = new Color(0.3f, 1f, 0.5f);
                    break;
                case HitJudgement.Good:
                    _judgementText.text = "Good";
                    _judgementText.color = new Color(0.9f, 0.9f, 0.3f);
                    break;
                case HitJudgement.Miss:
                    _judgementText.text = "MISS";
                    _judgementText.color = new Color(1f, 0.3f, 0.3f);
                    break;
            }
            _judgementTextTimer = 0.6f;
        }

        private void Update()
        {
            if (_judgementTextTimer > 0f)
            {
                _judgementTextTimer -= Time.unscaledDeltaTime;
                if (_judgementTextTimer <= 0f) _judgementText.text = "";
            }
        }

        private void ShowGameOver()
        {
            _gameOverOverlay.SetActive(true);
            _gameOverScoreText.text = $"Score: {Mathf.RoundToInt(GameManager.Instance.Score)}";
            var data = DancingBallOnBeats.Data.SaveSystem.Current;
            int best = data.GetHighScore(GameManager.Instance.CurrentSong.songId);
            _gameOverBestComboText.text = $"High Score: {best}";
        }
    }
}
