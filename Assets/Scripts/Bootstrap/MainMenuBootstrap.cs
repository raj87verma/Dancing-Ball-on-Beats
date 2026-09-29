using UnityEngine;
using UnityEngine.EventSystems;
using DancingBallOnBeats.UI;
using DancingBallOnBeats.Ads;

namespace DancingBallOnBeats.Bootstrap
{
    /// <summary>
    /// Entry point for the MainMenu scene. Attach this to a single empty GameObject in the
    /// scene (see Assets/Scenes/MainMenu.unity) — everything else (Canvas, EventSystem, all UI
    /// panels) is constructed here at runtime, so the scene file itself can stay minimal and
    /// never needs hand-edited GameObject hierarchies.
    /// </summary>
    public class MainMenuBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            EnsureEventSystem();
            EnsureAdsManager();

            Canvas canvas = UIFactory.CreateCanvas("MainMenuCanvas", out GameObject canvasRoot);
            canvasRoot.transform.SetParent(transform, false);

            MainMenuController.Build(canvasRoot.transform);
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private void EnsureAdsManager()
        {
            if (AdsManager.Instance != null) return;
            var go = new GameObject("AdsManager");
            go.AddComponent<AdsManager>().Initialize();
        }
    }
}
