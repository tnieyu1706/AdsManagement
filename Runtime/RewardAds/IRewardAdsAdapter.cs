using System;

namespace AdsManagement
{
    /// <summary>Interface for rewarded ad adapters. Adds placement, close, and reward events.</summary>
    public interface IRewardAdsAdapter : IAdsAdapter
    {
        void Show(string placement);
        bool IsPlacementCapped(string placement);
        event Action Closed;
        event Action<string, int> Rewarded;
    }
}
