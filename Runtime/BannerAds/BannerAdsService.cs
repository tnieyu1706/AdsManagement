using System;
using System.Threading.Tasks;

namespace AdsManagement
{
    /// <summary>
    /// Service for banner ads. Banners are persistent views with manual show/hide (void).
    /// No timeout/TCS unlike interstitial/rewarded because banners have no "completion" event.
    /// </summary>
    public class BannerAdsService : IDisposable
    {
        /// <summary>SDK communication adapter.</summary>
        private readonly IBannerAdsAdapter adapter;

        /// <summary>Minimum interval (seconds) between ad shows. 0 to disable pacing.</summary>
        public readonly float PacingTime;

        /// <summary>Timestamp of the last hide. Combine with PacingTime to calculate next available show time.</summary>
        public DateTime LastAdTime;

        public bool IsShowing { get; private set; }
        public bool IsLoadCompleted { get; set; }

        private TaskCompletionSource<bool> loadAdTcs;

        public event Action OnAdLoaded;
        public event Action OnAdLoadFailed;
        public event Action OnAdDisplayed;
        public event Action OnAdDisplayFailed;
        public event Action OnAdShown;
        public event Action OnAdHidden;

        public BannerAdsService(IBannerAdsAdapter adapter, float pacingTime) {
            this.adapter = adapter;
            PacingTime = pacingTime;
            RegisterHandlers();
        }

        /// <summary>Returns the current state: AdapterNotReady / Playing / Pacing / Ready.</summary>
        public AdState GetState() {
            if (!adapter.IsReady)
                return AdState.AdapterNotReady;
            if (IsShowing)
                return AdState.Playing;
            if (CheckPacing())
                return AdState.Pacing;
            return AdState.Ready;
        }

        /// <summary>Returns true if the pacing interval has not elapsed yet.</summary>
        public bool CheckPacing() {
            if (PacingTime <= 0) return false;
            var timeSinceLastAd = DateTime.Now - LastAdTime;
            return timeSinceLastAd.TotalSeconds < PacingTime;
        }

        #region Registers

        private void RegisterHandlers() {
            adapter.Loaded += HandleAdLoadComplete;
            adapter.LoadFailed += HandleAdLoadFailed;
            adapter.Displayed += HandleAdDisplayed;
            adapter.DisplayFailed += HandleAdDisplayFailed;
        }

        private void UnregisterHandlers() {
            adapter.Loaded -= HandleAdLoadComplete;
            adapter.LoadFailed -= HandleAdLoadFailed;
            adapter.Displayed -= HandleAdDisplayed;
            adapter.DisplayFailed -= HandleAdDisplayFailed;
        }

        #endregion

        #region Load Events

        private void HandleAdLoadComplete() {
            OnAdLoaded?.Invoke();
            loadAdTcs?.TrySetResult(true);
        }

        private void HandleAdLoadFailed() {
            OnAdLoadFailed?.Invoke();
            loadAdTcs?.TrySetResult(false);
        }

        /// <summary>Loads an ad. Automatically hides after load so callers control visibility via ShowAd/HideAd.</summary>
        public async Task<bool> LoadAd() {
            if (GetState() != AdState.Ready) return false;
            
            loadAdTcs?.TrySetResult(false);
            loadAdTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            adapter.Load();
            HideAd();
            var result = await loadAdTcs.Task;
            loadAdTcs = null;
            return result;
        }

        #endregion

        #region Show / Hide

        private void HandleAdDisplayed() {
            IsLoadCompleted = true;
            OnAdDisplayed?.Invoke();
        }

        private void HandleAdDisplayFailed() {
            OnAdDisplayFailed?.Invoke();
            IsLoadCompleted = false;
        }

        /// <summary>Shows the banner.</summary>
        public void ShowAd() {
            if (!IsLoadCompleted) return;
            
            IsShowing = true;
            OnAdShown?.Invoke();
            adapter.Show();
        }

        /// <summary>Hides the banner. Ad stays loaded and can be reshown.</summary>
        public void HideAd() {
            if (!IsLoadCompleted) return;
            
            IsShowing = false;
            OnAdHidden?.Invoke();
            adapter.Hide();
        }

        #endregion

        public void Dispose() {
            UnregisterHandlers();
            loadAdTcs?.TrySetCanceled();
        }
    }
}
