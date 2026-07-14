using System;
using System.Threading;
using System.Threading.Tasks;

namespace AdsManagement
{
    /// <summary>
    /// Service for interstitial ads. Provides async loading/showing with pacing and timeout.
    /// When the user closes the ad, returns Success (no reward unlike rewarded ads).
    /// </summary>
    public class InterstitialAdsService : IDisposable
    {
        /// <summary>SDK communication adapter.</summary>
        private readonly IInterstitialAdsAdapter adapter;

        /// <summary>Maximum wait time (ms) before timeout.</summary>
        public readonly int ShowTimeout;

        /// <summary>Minimum interval (seconds) between ad shows. 0 to disable pacing.</summary>
        public readonly float PacingTime;

        /// <summary>Timestamp of the last ad show. Combine with PacingTime to calculate next available show time.</summary>
        public DateTime LastAdTime;

        public bool ReadyToShow { get; set; }

        private TaskCompletionSource<AdResult> showAdTcs;
        private TaskCompletionSource<bool> loadAdTcs;
        private bool isShowing;

        public event Action OnAdLoaded;
        public event Action OnAdLoadFailed;
        public event Action OnAdDisplayed;
        public event Action OnAdDisplayFailed;
        public event Action OnAdClosed;

        public InterstitialAdsService(IInterstitialAdsAdapter adapter, float pacingTime, int showTimeout) {
            this.adapter = adapter;
            PacingTime = pacingTime;
            ShowTimeout = showTimeout;
            RegisterHandlers();
        }

        public bool IsPlacementCapped(string placement) => adapter.IsPlacementCapped(placement);

        /// <summary>Returns the current state: AdapterNotReady / Playing / Pacing / Ready.</summary>
        public AdState GetState() {
            if (!adapter.IsReady)
                return AdState.AdapterNotReady;
            if (isShowing)
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
            adapter.Closed += HandleAdClosed;
        }

        private void UnregisterHandlers() {
            adapter.Loaded -= HandleAdLoadComplete;
            adapter.LoadFailed -= HandleAdLoadFailed;
            adapter.Displayed -= HandleAdDisplayed;
            adapter.DisplayFailed -= HandleAdDisplayFailed;
            adapter.Closed -= HandleAdClosed;
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

        public async Task<bool> LoadAd() {
            if (GetState() != AdState.Ready) return false;

            loadAdTcs?.TrySetResult(false);
            loadAdTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            adapter.Load();
            var result = await loadAdTcs.Task;
            loadAdTcs = null;
            return result;
        }

        #endregion

        #region Show Events

        private void HandleAdDisplayed() {
            ReadyToShow = true;
            OnAdDisplayed?.Invoke();
        }

        private void HandleAdDisplayFailed() {
            OnAdDisplayFailed?.Invoke();
            ReadyToShow = false;
            showAdTcs?.TrySetResult(AdResult.Failed);
        }

        private void HandleAdClosed() {
            LastAdTime = DateTime.Now;
            if (showAdTcs == null) return;
            OnAdClosed?.Invoke();
            showAdTcs.TrySetResult(AdResult.Success);
        }

        /// <summary>
        /// Shows an interstitial ad and waits for the result.
        /// Returns Success on normal close, Failed on error, Timeout if exceeded ShowTimeout.
        /// </summary>
        /// <param name="placement">Placement name. null = default.</param>
        /// <param name="token">Cancellation token.</param>
        public async Task<AdResult> ShowAd(string placement = null, CancellationToken token = default) {
            if (!ReadyToShow) return AdResult.Failed;

            if (isShowing) return AdResult.Failed;

            showAdTcs?.TrySetResult(AdResult.Failed);
            isShowing = true;
            showAdTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var registration = token.Register(() => showAdTcs?.TrySetCanceled(token));
            adapter.Show(placement);

            AdResult result;
            try {
                var taskComplete = await Task.WhenAny(showAdTcs.Task, Task.Delay(ShowTimeout, token));
                if (taskComplete != showAdTcs.Task) {
                    result = AdResult.Timeout;
                } else {
                    result = await (Task<AdResult>)taskComplete;
                }
            }
            catch (OperationCanceledException) {
                return AdResult.Failed;
            }

            showAdTcs = null;
            ReadyToShow = false;
            isShowing = false;
            return result;
        }

        #endregion

        public void Dispose() {
            UnregisterHandlers();
            loadAdTcs?.TrySetCanceled();
            showAdTcs?.TrySetCanceled();
        }
    }
}