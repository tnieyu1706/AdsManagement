using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace AdsManagement
{
    public class InterstitialAdService : BaseAdService
    {
        private TaskCompletionSource<AdResult> tcs;
        private readonly int timeout;

        public override bool IsReady => Ads?.InterstitialAd != null && Ads.InterstitialAd.IsAdReady();

        public InterstitialAdService(AdsInstance ads, int timeout = 15000) : base(ads) {
            RegisterHandlers();
            this.timeout = timeout;
        }

        private void RegisterHandlers() {
            if (Ads?.InterstitialAd == null) return;

            var interstitialAd = Ads.InterstitialAd;

            interstitialAd.OnAdClosed += OnAdClosed;
            interstitialAd.OnAdDisplayFailed += OnAdDisplayFailed;
        }

        private void UnregisterHandlers() {
            if (Ads?.InterstitialAd == null) return;

            var interstitialAd = Ads.InterstitialAd;

            interstitialAd.OnAdClosed -= OnAdClosed;
            interstitialAd.OnAdDisplayFailed -= OnAdDisplayFailed;
        }

        private void OnAdClosed(LevelPlayAdInfo info) {
            SetTcsWithState(AdResult.Completed);
        }

        private void OnAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error) {
            Debug.Log($"[InterstitialAdService] Ad display failed: {error}");
            SetTcsWithState(AdResult.Failed);
        }

        private void SetTcsWithState(AdResult state) {
            tcs?.TrySetResult(state);
            tcs = null;
        }

        public override void LoadAd() {
            Ads.InterstitialAd?.LoadAd();
        }

        public override async Task<AdResult> ShowAsync(CancellationToken cancellationToken = default) {
            if (!IsReady) return AdResult.NotReady;

            tcs = new TaskCompletionSource<AdResult>();
            await using var registration = cancellationToken.Register(() =>
            {
                tcs?.TrySetCanceled(cancellationToken);
            });

            Ads.InterstitialAd.ShowAd();

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