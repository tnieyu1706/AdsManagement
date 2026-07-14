#if ADSMANAGEMENT_LEVELPLAY_SUPPORT
using System;
using Unity.Services.LevelPlay;

namespace AdsManagement.LevelPlayService
{
    public class LevelPlayInterstitialAdsAdapter : IInterstitialAdsAdapter
    {
        public LevelPlayInterstitialAd InterstitialAd;
        private readonly string appKey;
        private readonly string interstitialUnitId;

        public bool IsReady => InterstitialAd != null;

        public LevelPlayInterstitialAdsAdapter(string appKey, string interstitialUnitId) {
            this.appKey = appKey;
            this.interstitialUnitId = interstitialUnitId;
        }

        public void Initialize() {
            if (!LevelPlayConfig.IsInitialized) {
                LevelPlayConfig.Initialize(appKey);
            }

            InterstitialAd = new LevelPlayInterstitialAd(interstitialUnitId);
            RegisterHandlers();
        }

        public void Load() {
            InterstitialAd.LoadAd();
        }

        public void Show(string placement) {
            InterstitialAd.ShowAd(placement);
        }

        public bool IsPlacementCapped(string placement) {
            return LevelPlayInterstitialAd.IsPlacementCapped(placement);
        }

        public event Action Loaded;
        public event Action LoadFailed;
        public event Action Displayed;
        public event Action DisplayFailed;
        public event Action Closed;

        #region Register Handlers

        private void RegisterHandlers() {
            if (InterstitialAd == null) return;

            InterstitialAd.OnAdLoaded += HandleAdLoadComplete;
            InterstitialAd.OnAdLoadFailed += HandleAdLoadFailed;
            InterstitialAd.OnAdDisplayed += HandleAdDisplayComplete;
            InterstitialAd.OnAdDisplayFailed += HandleAdDisplayFailed;
            InterstitialAd.OnAdClosed += HandleAdClosed;
        }

        private void HandleAdLoadComplete(LevelPlayAdInfo info) {
            Loaded?.Invoke();
        }

        private void HandleAdLoadFailed(LevelPlayAdError error) {
            LoadFailed?.Invoke();
        }

        private void HandleAdDisplayComplete(LevelPlayAdInfo info) {
            Displayed?.Invoke();
        }

        private void HandleAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error) {
            DisplayFailed?.Invoke();
        }

        private void HandleAdClosed(LevelPlayAdInfo info) {
            Closed?.Invoke();
        }

        #endregion
    }
}
#endif