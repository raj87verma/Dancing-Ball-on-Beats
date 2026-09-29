using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DancingBallOnBeats.Data;

namespace DancingBallOnBeats.UI
{
    /// <summary>
    /// Simple settings screen: music/SFX volume sliders + mute toggles, backed directly by
    /// SaveSystem so changes persist immediately.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        public event Action OnBack;
        private GameObject _panel;

        public static SettingsController Build(Transform canvasRoot)
        {
            var go = new GameObject("SettingsController");
            go.transform.SetParent(canvasRoot, false);
            var controller = go.AddComponent<SettingsController>();
            controller.BuildUI(canvasRoot);
            return controller;
        }

        public void SetVisible(bool visible) => _panel.SetActive(visible);

        private void BuildUI(Transform canvasRoot)
        {
            _panel = UIFactory.CreatePanel(canvasRoot, "SettingsPanel", new Color(0.05f, 0.05f, 0.09f, 1f), Vector2.zero, Vector2.one);

            UIFactory.CreateText(_panel.transform, "HeaderText", "Settings", 56, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.96f));

            var backBtn = UIFactory.CreateButton(_panel.transform, "BackButton", "< Back",
                new Color(0.2f, 0.2f, 0.25f), Color.white,
                new Vector2(0.03f, 0.9f), new Vector2(0.22f, 0.97f), fontSize: 30);
            backBtn.onClick.AddListener(() => OnBack?.Invoke());

            var data = SaveSystem.Current;

            UIFactory.CreateText(_panel.transform, "MusicLabel", "Music Volume", 34, Color.white,
                TextAlignmentOptions.Left, new Vector2(0.08f, 0.72f), new Vector2(0.6f, 0.78f));
            var musicSlider = UIFactory.CreateSlider(_panel.transform, "MusicSlider", data.musicVolume,
                new Vector2(0.08f, 0.65f), new Vector2(0.92f, 0.71f));
            musicSlider.onValueChanged.AddListener(v => SaveSystem.SetVolumes(v, SaveSystem.Current.sfxVolume));

            UIFactory.CreateText(_panel.transform, "SfxLabel", "SFX Volume", 34, Color.white,
                TextAlignmentOptions.Left, new Vector2(0.08f, 0.55f), new Vector2(0.6f, 0.61f));
            var sfxSlider = UIFactory.CreateSlider(_panel.transform, "SfxSlider", data.sfxVolume,
                new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.54f));
            sfxSlider.onValueChanged.AddListener(v => SaveSystem.SetVolumes(SaveSystem.Current.musicVolume, v));

            UIFactory.CreateText(_panel.transform, "CreditsHeader", "Music Credits", 30, new Color(0.7f, 0.7f, 0.75f),
                TextAlignmentOptions.Left, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.42f));

            UIFactory.CreateText(_panel.transform, "CreditsBody",
                "Backbeat, Bit Bit Loop & Hippety Hop: Public Domain (CC0 1.0), freepd.com.\n" +
                "Pixel Dash, Calm Orbit & Neon Rush: original procedural audio generated on-device.",
                24, new Color(0.6f, 0.6f, 0.65f), TextAlignmentOptions.Left,
                new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.36f));
        }
    }
}
