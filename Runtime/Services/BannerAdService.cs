using System.Threading;
using System.Threading.Tasks;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace AdsManagement
{
    public class BannerAdService : BaseAdService
    {
        private TaskCompletionSource<AdResult> tcs;

        private bool isReady;
        public sealed override bool IsReady => isReady;

        public BannerAdService(AdsInstance ads) : base(ads) {
            RegisterHandlers();
        }

        private void RegisterHandlers() {
            if (Ads?.BannerAd == null) return;
            
            var bannerAd = Ads.BannerAd;

            bannerAd.OnAdDisplayed += OnBannerAdLoaded;
            bannerAd.OnAdDisplayFailed += OnBannerAdLoadFailed;
        }

        private void UnregisterHandlers() {
            if (Ads?.BannerAd == null) return;
            
            var bannerAd = Ads.BannerAd;

            bannerAd.OnAdDisplayed -= OnBannerAdLoaded;
            bannerAd.OnAdDisplayFailed -= OnBannerAdLoadFailed;
        }

        private void OnBannerAdLoaded(LevelPlayAdInfo info) {
            isReady = true;
        }

        private void OnBannerAdLoadFailed(LevelPlayAdInfo info, LevelPlayAdError error) {
            Debug.Log($"[BannerAdService] Banner ad load failed: {error}");
            isReady = false;
        }

        public override void LoadAd() {
            Ads.BannerAd?.LoadAd();
        }

        public override Task<AdResult> ShowAsync(CancellationToken cancellationToken = default) {
            if (!IsReady) return Task.FromResult(AdResult.NotReady);

            Ads.BannerAd.ShowAd();
            return Task.FromResult(AdResult.Completed);
        }

        public void Show() {
            Ads.BannerAd.ShowAd();
        }

        public void Hide() {
            Ads.BannerAd.HideAd();
        }

        public override void Dispose() {
            UnregisterHandlers();
        }
    }
}