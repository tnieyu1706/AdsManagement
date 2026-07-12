using System;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace AdsManagement
{
    public class AdsInstance : IDisposable
    {
        private AdsConfiguration adsConfiguration;
        
        public LevelPlayBannerAd BannerAd;
        public LevelPlayInterstitialAd InterstitialAd;
        public LevelPlayRewardedAd RewardedAd;
        
        public AdsInstance(AdsConfiguration adsConfiguration) {
            this.adsConfiguration = adsConfiguration;
        }

        public void Init() {
            Debug.Log("[AdsController].LevelPlay - Start setup");
            LevelPlaySetup();
            Debug.Log("[AdsController].LevelPlay - Setup completed");

            // Ads Setup
            Debug.Log("[AdsController].Ads - Start setup");
            SetupAds();
            Debug.Log("[AdsController].Ads - Setup completed");
        }
        
        private void LevelPlaySetup() {
            LevelPlay.ValidateIntegration();
            LevelPlay.Init(adsConfiguration.appKey);
        }

        private void SetupAds() {

            if (!string.IsNullOrEmpty(adsConfiguration.bannerUnitId)) {
                BannerAd = new LevelPlayBannerAd(adsConfiguration.bannerUnitId);
                BannerAd.OnAdLoaded += OnBannerLoaded;
                BannerAd.OnAdLoadFailed += OnBannerLoadFailed;
            }

            if (!string.IsNullOrEmpty(adsConfiguration.interstitialUnitId)) {
                InterstitialAd = new LevelPlayInterstitialAd(adsConfiguration.interstitialUnitId);
                InterstitialAd.OnAdLoaded += OnInterstitialLoaded;
                InterstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
            }

            if (!string.IsNullOrEmpty(adsConfiguration.rewardedVideoUnitId)) {
                RewardedAd = new LevelPlayRewardedAd(adsConfiguration.rewardedVideoUnitId);
                RewardedAd.OnAdLoaded += OnRewardedLoaded;
                RewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
            }
        }
        
        void UnregisterEvents() {
            if (BannerAd != null) {
                BannerAd.OnAdLoaded -= OnBannerLoaded;
                BannerAd.OnAdLoadFailed -= OnBannerLoadFailed;
            }

            if (InterstitialAd != null) {
                InterstitialAd.OnAdLoaded -= OnInterstitialLoaded;
                InterstitialAd.OnAdLoadFailed -= OnInterstitialLoadFailed;
            }

            if (RewardedAd != null) {
                RewardedAd.OnAdLoaded -= OnRewardedLoaded;
                RewardedAd.OnAdLoadFailed -= OnRewardedLoadFailed;
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

        public void Dispose() {
            UnregisterEvents();
        }
    }
}