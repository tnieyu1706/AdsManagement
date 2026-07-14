#if ADSMANAGEMENT_LEVELPLAY_SUPPORT
using System;
using Unity.Services.LevelPlay;

namespace AdsManagement.LevelPlayService
{
    public class LevelPlayRewardAdsAdapter : IRewardAdsAdapter
    {
        public LevelPlayRewardedAd RewardAd;
        private readonly string appKey;
        private readonly string rewardUnitId;

        public bool IsReady => RewardAd != null;

        public LevelPlayRewardAdsAdapter(string appKey, string rewardUnitId) {
            this.appKey = appKey;
            this.rewardUnitId = rewardUnitId;
        }

        public void Initialize() {
            if (!LevelPlayConfig.IsInitialized) {
                LevelPlayConfig.Initialize(appKey);
            }

            RewardAd = new LevelPlayRewardedAd(rewardUnitId);
            RegisterHandlers();
        }

        public void Load() {
            RewardAd.LoadAd();
        }

        public void Show(string placement) {
            RewardAd.ShowAd(placement);
        }

        public bool IsPlacementCapped(string placement) {
            return LevelPlayRewardedAd.IsPlacementCapped(placement);
        }

        public event Action Loaded;
        public event Action LoadFailed;
        public event Action Displayed;
        public event Action DisplayFailed;
        public event Action<string, int> Rewarded;
        public event Action Closed;

        #region Register Handlers

        private void RegisterHandlers() {
            if (RewardAd == null) return;

            RewardAd.OnAdLoaded += HandleAdLoadComplete;
            RewardAd.OnAdLoadFailed += HandleAdLoadFailed;
            RewardAd.OnAdDisplayed += HandleAdDisplayComplete;
            RewardAd.OnAdDisplayFailed += HandleAdDisplayFailed;
            RewardAd.OnAdRewarded += HandleAdGetReward;
            RewardAd.OnAdClosed += HandleAdClosed;
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

        private void HandleAdGetReward(LevelPlayAdInfo info, LevelPlayReward reward) {
            Rewarded?.Invoke(reward.Name, reward.Amount);
        }

        private void HandleAdClosed(LevelPlayAdInfo info) {
            Closed?.Invoke();
        }

        #endregion
    }
}
#endif