using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DancingBallOnBeats.Audio;
using DancingBallOnBeats.Core;
using DancingBallOnBeats.Data;

namespace DancingBallOnBeats.UI
{
    /// <summary>
    /// Scrollable-feeling list (simple stacked rows, no ScrollRect needed for 6 songs) of every
    /// song in AudioTrackLibrary. Tapping a row stores the choice in GameSession and loads the
    /// Gameplay scene.
    /// </summary>
    public class SongSelectController : MonoBehaviour
    {
        public event Action OnBack;
        private GameObject _panel;

        public static SongSelectController Build(Transform canvasRoot)
        {
            var go = new GameObject("SongSelectController");
            go.transform.SetParent(canvasRoot, false);
            var controller = go.AddComponent<SongSelectController>();
            controller.BuildUI(canvasRoot);
            return controller;
        }

        public void SetVisible(bool visible) => _panel.SetActive(visible);

        private void BuildUI(Transform canvasRoot)
        {
            _panel = UIFactory.CreatePanel(canvasRoot, "SongSelectPanel", new Color(0.05f, 0.05f, 0.09f, 1f), Vector2.zero, Vector2.one);

            UIFactory.CreateText(_panel.transform, "HeaderText", "Select a Track", 56, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.96f));

            var backBtn = UIFactory.CreateButton(_panel.transform, "BackButton", "< Back",
                new Color(0.2f, 0.2f, 0.25f), Color.white,
                new Vector2(0.03f, 0.9f), new Vector2(0.22f, 0.97f), fontSize: 30);
            backBtn.onClick.AddListener(() => OnBack?.Invoke());

            List<SongData> library = AudioTrackLibrary.BuildDefaultLibrary();

            float rowHeight = 0.115f;
            float startY = 0.82f;
            for (int i = 0; i < library.Count; i++)
            {
                SongData song = library[i];
                float top = startY - i * rowHeight;
                float bottom = top - (rowHeight - 0.015f);
                BuildSongRow(song, top, bottom);
            }
        }

        private void BuildSongRow(SongData song, float anchorTop, float anchorBottom)
        {
            var row = UIFactory.CreatePanel(_panel.transform, $"Row_{song.songId}", new Color(0.12f, 0.13f, 0.2f, 1f),
                new Vector2(0.05f, anchorBottom), new Vector2(0.95f, anchorTop));

            var button = row.AddComponent<UnityEngine.UI.Button>();
            var image = row.GetComponent<UnityEngine.UI.Image>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = song.themeColor * 0.35f + new Color(0f, 0f, 0f, 0.65f);
            colors.highlightedColor = song.themeColor * 0.5f;
            colors.pressedColor = song.themeColor * 0.25f;
            image.color = colors.normalColor;
            button.colors = colors;

            UIFactory.CreateText(row.transform, "NameText", song.displayName, 38, Color.white,
                TextAlignmentOptions.Left, new Vector2(0.04f, 0.15f), new Vector2(0.65f, 0.9f));

            string modeLabel = song.sourceMode == SongSourceMode.RealTrack ? "Licensed CC0 Track" : "Procedural (No File)";
            UIFactory.CreateText(row.transform, "ModeText", modeLabel, 22, new Color(0.75f, 0.75f, 0.8f),
                TextAlignmentOptions.Left, new Vector2(0.04f, 0.0f), new Vector2(0.65f, 0.28f));

            int best = SaveSystem.Current.GetHighScore(song.songId);
            UIFactory.CreateText(row.transform, "HighScoreText", $"Best: {best}", 26, new Color(1f, 0.85f, 0.3f),
                TextAlignmentOptions.Right, new Vector2(0.65f, 0.55f), new Vector2(0.95f, 0.9f));

            UIFactory.CreateText(row.transform, "DifficultyText", new string('★', song.difficulty), 26, song.themeColor,
                TextAlignmentOptions.Right, new Vector2(0.65f, 0.1f), new Vector2(0.95f, 0.5f));

            button.onClick.AddListener(() =>
            {
                GameSession.SelectedSongId = song.songId;
                SceneManager.LoadScene("Gameplay");
            });
        }
    }
}
