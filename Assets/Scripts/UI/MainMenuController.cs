using UnityEngine;
using TMPro;
using DancingBallOnBeats.Core;

namespace DancingBallOnBeats.UI
{
    /// <summary>
    /// Root screen of the Main Menu scene: title, Play / Settings buttons, and hosts the
    /// SongSelect and Settings panels (built by their own controllers) which slide in on top.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private GameObject _homePanel;
        private SongSelectController _songSelect;
        private SettingsController _settings;

        public static MainMenuController Build(Transform canvasRoot)
        {
            var go = new GameObject("MainMenuController");
            go.transform.SetParent(canvasRoot, false);
            var controller = go.AddComponent<MainMenuController>();
            controller.BuildUI(canvasRoot);
            return controller;
        }

        private void BuildUI(Transform canvasRoot)
        {
            _homePanel = UIFactory.CreatePanel(canvasRoot, "HomePanel", new Color(0.06f, 0.07f, 0.12f, 1f), Vector2.zero, Vector2.one);

            UIFactory.CreateText(_homePanel.transform, "TitleText", "Dancing Ball\non Beats", 84, new Color(0.35f, 0.9f, 1f),
                TextAlignmentOptions.Center, new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.85f));

            UIFactory.CreateText(_homePanel.transform, "SubtitleText", "Bounce to the rhythm. Dodge the beat.", 32, new Color(0.8f, 0.8f, 0.85f),
                TextAlignmentOptions.Center, new Vector2(0.05f, 0.56f), new Vector2(0.95f, 0.62f));

            var playBtn = UIFactory.CreateButton(_homePanel.transform, "PlayButton", "PLAY",
                new Color(0.25f, 0.85f, 0.5f), Color.white,
                new Vector2(0.2f, 0.38f), new Vector2(0.8f, 0.5f), fontSize: 54);
            playBtn.onClick.AddListener(OpenSongSelect);

            var settingsBtn = UIFactory.CreateButton(_homePanel.transform, "SettingsButton", "Settings",
                new Color(0.25f, 0.3f, 0.4f), Color.white,
                new Vector2(0.2f, 0.24f), new Vector2(0.8f, 0.35f));
            settingsBtn.onClick.AddListener(OpenSettings);

            UIFactory.CreateText(_homePanel.transform, "VersionText", "v1.0 - Free Edition", 22, new Color(0.5f, 0.5f, 0.55f),
                TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(1f, 0.06f));

            _songSelect = SongSelectController.Build(canvasRoot);
            _songSelect.SetVisible(false);
            _songSelect.OnBack += () => { _songSelect.SetVisible(false); _homePanel.SetActive(true); };

            _settings = SettingsController.Build(canvasRoot);
            _settings.SetVisible(false);
            _settings.OnBack += () => { _settings.SetVisible(false); _homePanel.SetActive(true); };
        }

        private void OpenSongSelect()
        {
            _homePanel.SetActive(false);
            _songSelect.SetVisible(true);
        }

        private void OpenSettings()
        {
            _homePanel.SetActive(false);
            _settings.SetVisible(true);
        }
    }
}
