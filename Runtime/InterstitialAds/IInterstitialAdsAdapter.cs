using System;

namespace AdsManagement
{
    /// <summary>Interface for interstitial ad adapters. Adds show, placement capping, and close support.</summary>
    public interface IInterstitialAdsAdapter : IAdsAdapter
    {
        void Show(string placement);
        bool IsPlacementCapped(string placement);
        event Action Closed;
    }
}
