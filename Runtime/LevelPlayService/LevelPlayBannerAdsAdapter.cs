#if ADSMANAGEMENT_LEVELPLAY_SUPPORT
using System;
using Unity.Services.LevelPlay;

namespace AdsManagement.LevelPlayService
{
    public class LevelPlayBannerAdsAdapter : IBannerAdsAdapter
    {
        public LevelPlayBannerAd BannerAd;
        private readonly string appKey;
        private readonly string bannerUnitId;

        public bool IsReady => BannerAd != null;

        public LevelPlayBannerAdsAdapter(string appKey, string bannerUnitId) {
            this.appKey = appKey;
            this.bannerUnitId = bannerUnitId;
        }

        public void Initialize() {
            if (!LevelPlayConfig.IsInitialized) {
                LevelPlayConfig.Initialize(appKey);
            }

            BannerAd = new LevelPlayBannerAd(bannerUnitId);
            RegisterHandlers();
        }

        public void Load() {
            BannerAd.LoadAd();
        }

        public void Show() {
            BannerAd.ShowAd();
        }

        public void Hide() {
            BannerAd.HideAd();
        }

        public event Action Loaded;
        public event Action LoadFailed;
        public event Action Displayed;
        public event Action DisplayFailed;

        #region Register Handlers

        private void RegisterHandlers() {
            if (BannerAd == null) return;

            BannerAd.OnAdLoaded += HandleAdLoadComplete;
            BannerAd.OnAdLoadFailed += HandleAdLoadFailed;
            BannerAd.OnAdDisplayed += HandleAdDisplayComplete;
            BannerAd.OnAdDisplayFailed += HandleAdDisplayFailed;
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

        #endregion
    }
}
#endif