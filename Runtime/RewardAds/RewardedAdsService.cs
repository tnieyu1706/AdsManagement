using System;
using System.Threading;
using System.Threading.Tasks;

namespace AdsManagement
{
    /// <summary>
    /// Service for rewarded video ads. Provides async loading/showing with pacing, timeout, and reward handling.
    /// </summary>
    public class RewardedAdsService : IDisposable
    {
        private const int REWARD_BUFFER_MS = 300;

        /// <summary>SDK adapter.</summary>
        private readonly IRewardAdsAdapter adapter;

        /// <summary>Timeout (ms).</summary>
        public readonly int ShowTimeout;

        /// <summary>Pacing interval (seconds). 0 = disabled.</summary>
        public readonly float PacingTime;

        /// <summary>Last ad timestamp. Use with PacingTime to calculate next available show.</summary>
        public DateTime LastAdTime;

        public bool ReadyToShow { get; set; }

        private TaskCompletionSource<(AdResult, Reward?)> showAdTcs;
        private TaskCompletionSource<bool> loadAdTcs;

        private bool isCollectReward;
        private bool isShowing;

        public event Action OnAdLoaded;
        public event Action OnAdLoadFailed;
        public event Action OnAdDisplayed;
        public event Action OnAdDisplayFailed;
        public event Action<string, int> OnGetReward;
        public event Action OnAdClosed;

        public RewardedAdsService(IRewardAdsAdapter adapter, float pacingTime, int showTimeout) {
            this.adapter = adapter;
            PacingTime = pacingTime;
            ShowTimeout = showTimeout;
            RegisterHandlers();
        }

        public bool IsPlacementCapped(string placement) => adapter.IsPlacementCapped(placement);

        /// <summary>Returns the current ad state (AdapterNotReady / Playing / Pacing / Ready).</summary>
        public AdState GetState() {
            if (!adapter.IsReady)
                return AdState.AdapterNotReady;
            if (isShowing)
                return AdState.Playing;
            if (CheckPacing())
                return AdState.Pacing;
            return AdState.Ready;
        }

        /// <summary>True if the pacing interval has not elapsed yet.</summary>
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
            adapter.Rewarded += HandleAdGetReward;
            adapter.Closed += HandleAdClosed;
        }

        private void UnregisterHandlers() {
            adapter.Loaded -= HandleAdLoadComplete;
            adapter.LoadFailed -= HandleAdLoadFailed;
            adapter.Displayed -= HandleAdDisplayed;
            adapter.DisplayFailed -= HandleAdDisplayFailed;
            adapter.Rewarded -= HandleAdGetReward;
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
            showAdTcs?.TrySetResult((AdResult.Failed, null));
        }

        private void HandleAdGetReward(string name, int amount) {
            if (showAdTcs == null) return;
            isCollectReward = true;
            var reward = new Reward() { Name = name, Amount = amount };
            OnGetReward?.Invoke(name, reward.Amount);
            showAdTcs.TrySetResult((AdResult.Success, reward));
        }

        private async void HandleAdClosed() {
            LastAdTime = DateTime.Now;
            if (showAdTcs == null) return;
            if (!isCollectReward)
                await Task.Delay(REWARD_BUFFER_MS);
            if (!isCollectReward) {
                OnAdClosed?.Invoke();
                showAdTcs.TrySetResult((AdResult.Interrupted, null));
            }
        }

        /// <summary>
        /// Shows the ad and returns (AdResult, Reward?).
        /// Reward is non-null only if AdResult == Success.
        /// </summary>
        /// <param name="placement">Placement name.</param>
        /// <param name="token">Cancellation token.</param>
        public async Task<(AdResult, Reward?)> ShowAd(string placement = null, CancellationToken token = default) {
            if (!ReadyToShow) return (AdResult.Failed, null);

            if (isShowing) return (AdResult.Failed, null);

            showAdTcs?.TrySetResult((AdResult.Failed, null));
            isShowing = true;
            showAdTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            isCollectReward = false;

            await using var registration = token.Register(() => showAdTcs?.TrySetCanceled(token));
            adapter.Show(placement);

            (AdResult state, Reward? reward) result;
            try {
                var taskComplete = await Task.WhenAny(showAdTcs.Task, Task.Delay(ShowTimeout, token));
                if (taskComplete != showAdTcs.Task) {
                    result = (AdResult.Timeout, null);
                } else {
                    result = await (Task<(AdResult, Reward?)>)taskComplete;
                }
            }
            catch (OperationCanceledException) {
                return (AdResult.Failed, null);
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