using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace AdsManagement
{
    public class RewardedAdService : BaseAdService
    {
        private AdResult state;
        private TaskCompletionSource<AdResult> tcs;
        private readonly int timeout;

        private bool isCollectReward;

        public override bool IsReady => Ads?.RewardedAd != null && Ads.RewardedAd.IsAdReady();

        public RewardedAdService(AdsInstance ads, int timeout = 15000) : base(ads) {
            RegisterHandlers();
            this.timeout = timeout;
        }

        private void RegisterHandlers() {
            if (Ads?.RewardedAd == null) return;
            
            var rewardedAd = Ads.RewardedAd;

            rewardedAd.OnAdDisplayFailed += OnAdDisplayedFailed;
            rewardedAd.OnAdRewarded += OnAdRewarded;
            rewardedAd.OnAdClosed += OnAdClosed;
        }

        private void UnregisterHandlers() {
            if (Ads?.RewardedAd == null) return;
            
            var rewardedAd = Ads.RewardedAd;

            rewardedAd.OnAdDisplayFailed -= OnAdDisplayedFailed;
            rewardedAd.OnAdRewarded -= OnAdRewarded;
            rewardedAd.OnAdClosed -= OnAdClosed;
        }

        private void OnAdDisplayedFailed(LevelPlayAdInfo info, LevelPlayAdError error) {
            Debug.Log($"[RewardedAdService] Ad display failed: {error}");
            SetTcsWithState(AdResult.Failed);
        }

        private void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward) {
            isCollectReward = true;
            SetTcsWithState(AdResult.Completed);
        }

        private async void OnAdClosed(LevelPlayAdInfo info) {
            if (tcs == null) return;

            if (!isCollectReward)
                await Task.Delay(300); // delay buffer for reward callback to be called before closing the ad

            SetTcsWithState(isCollectReward ? AdResult.Completed : AdResult.Closed);
        }

        private void SetTcsWithState(AdResult stateSource) {
            tcs?.TrySetResult(stateSource);
            tcs = null;
        }

        public override void LoadAd() {
            Ads.RewardedAd?.LoadAd();
        }

        public override async Task<AdResult> ShowAsync(CancellationToken cancellationToken = default) {
            isCollectReward = false;
            if (!IsReady) return AdResult.NotReady;

            tcs = new TaskCompletionSource<AdResult>();
            await using var registration = cancellationToken.Register(() => {
                tcs?.TrySetCanceled(cancellationToken);
            });
            
            Ads.RewardedAd.ShowAd();

            try {
                var completed = await Task.WhenAny(tcs.Task, Task.Delay(timeout, cancellationToken));
                
                if (completed != tcs.Task) {
                    tcs = null;
                    return AdResult.Timeout;
                }
                return await (Task<AdResult>)completed;
            }
            catch (OperationCanceledException) {
                tcs = null;
                return AdResult.Cancelled;
            }
        }

        public override void Dispose() {
            UnregisterHandlers();
            SetTcsWithState(AdResult.Failed);
        }
    }
}