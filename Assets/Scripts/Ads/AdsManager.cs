using System;
using UnityEngine;

namespace DancingBallOnBeats.Ads
{
    /// <summary>
    /// No-op placeholder ad service for the free v1. Every method logs and immediately invokes
    /// its completion callback, so the rest of the game (e.g. "watch ad to revive") can be wired
    /// up now and later swapped for a real SDK (Google AdMob / Unity Ads / LevelPlay) by replacing
    /// only the body of these methods — no call sites elsewhere need to change.
    ///
    /// To integrate AdMob later:
    /// 1. Install the Google Mobile Ads Unity plugin (see https://developers.google.com/admob/unity/quick-start).
    /// 2. Replace the bodies below with real Banner/Interstitial/Rewarded ad calls.
    /// 3. Add your AdMob App ID to Assets/Plugins/Android/AndroidManifest.xml (meta-data tag).
    /// </summary>
    public class AdsManager : MonoBehaviour
    {
        public static AdsManager Instance { get; private set; }

        public bool AdsEnabled { get; set; } = false; // off by default for the ad-free v1 scope

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize()
        {
            Debug.Log("[AdsManager] Stub initialized. No real ad SDK is wired up yet (AdsEnabled=" + AdsEnabled + ").");
        }

        public void ShowBanner()
        {
            if (!AdsEnabled) return;
            Debug.Log("[AdsManager] ShowBanner() called - stub, no real banner shown.");
        }

        public void HideBanner()
        {
            if (!AdsEnabled) return;
            Debug.Log("[AdsManager] HideBanner() called - stub.");
        }

        public void ShowInterstitial(Action onClosed)
        {
            Debug.Log("[AdsManager] ShowInterstitial() called - stub, completing immediately.");
            onClosed?.Invoke();
        }

        /// <summary>Used for e.g. "watch an ad to get an extra life". Always grants the reward in this stub.</summary>
        public void ShowRewarded(Action<bool> onCompleted)
        {
            Debug.Log("[AdsManager] ShowRewarded() called - stub, granting reward immediately.");
            onCompleted?.Invoke(true);
        }
    }
}
