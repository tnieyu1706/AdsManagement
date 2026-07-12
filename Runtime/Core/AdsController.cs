using Unity.Services.LevelPlay;
using UnityEngine;

namespace AdsManagement
{
    [DefaultExecutionOrder(-100)]
    public class AdsController : MonoBehaviour
    {
        [SerializeField] private AdsConfiguration adsConfiguration;

        public readonly AdsSet Ads = new AdsSet();

        void Awake() {
            Debug.Log("[AdsController].LevelPlay - Start setup");
            LevelPlaySetup();
            Debug.Log("[AdsController].LevelPlay - Setup completed");

            // Ads Setup
            Debug.Log("[AdsController].Ads - Start setup");
            SetupAds();
            Debug.Log("[AdsController].Ads - Setup completed");
        }

        private void OnDestroy() {
            UnregisterEvents();
        }

        private void LevelPlaySetup() {
            LevelPlay.ValidateIntegration();
            LevelPlay.Init(adsConfiguration.appKey);
        }

        private void SetupAds() {

            if (!string.IsNullOrEmpty(adsConfiguration.bannerUnitId)) {
                Ads.BannerAd = new LevelPlayBannerAd(adsConfiguration.bannerUnitId);
                Ads.BannerAd.OnAdLoaded += OnBannerLoaded;
                Ads.BannerAd.OnAdLoadFailed += OnBannerLoadFailed;
            }

            if (!string.IsNullOrEmpty(adsConfiguration.interstitialUnitId)) {
                Ads.InterstitialAd = new LevelPlayInterstitialAd(adsConfiguration.interstitialUnitId);
                Ads.InterstitialAd.OnAdLoaded += OnInterstitialLoaded;
                Ads.InterstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
            }

            if (!string.IsNullOrEmpty(adsConfiguration.rewardedVideoUnitId)) {
                Ads.RewardedAd = new LevelPlayRewardedAd(adsConfiguration.rewardedVideoUnitId);
                Ads.RewardedAd.OnAdLoaded += OnRewardedLoaded;
                Ads.RewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
            }
        }

        void UnregisterEvents() {
            if (Ads.BannerAd != null) {
                Ads.BannerAd.OnAdLoaded -= OnBannerLoaded;
                Ads.BannerAd.OnAdLoadFailed -= OnBannerLoadFailed;
            }

            if (Ads.InterstitialAd != null) {
                Ads.InterstitialAd.OnAdLoaded -= OnInterstitialLoaded;
                Ads.InterstitialAd.OnAdLoadFailed -= OnInterstitialLoadFailed;
            }

            if (Ads.RewardedAd != null) {
                Ads.RewardedAd.OnAdLoaded -= OnRewardedLoaded;
                Ads.RewardedAd.OnAdLoadFailed -= OnRewardedLoadFailed;
            }
        }

        #region Ad Events

        private void OnBannerLoaded(LevelPlayAdInfo info) {
            Debug.Log($"[AdsController] Banner ad loaded: {info}");
        }

        private void OnBannerLoadFailed(LevelPlayAdError error) {
            Debug.Log($"[AdsController] Banner ad load failed: {error}");
        }

        private void OnInterstitialLoaded(LevelPlayAdInfo info) {
            Debug.Log($"[AdsController] Interstitial ad loaded: {info}");
        }

        private void OnInterstitialLoadFailed(LevelPlayAdError error) {
            Debug.Log($"[AdsController] Interstitial ad load failed: {error}");
        }

        private void OnRewardedLoaded(LevelPlayAdInfo info) {
            Debug.Log($"[AdsController] Rewarded ad loaded: {info}");
        }

        private void OnRewardedLoadFailed(LevelPlayAdError error) {
            Debug.Log($"[AdsController] Rewarded ad load failed: {error}");
        }

        #endregion
    }
}