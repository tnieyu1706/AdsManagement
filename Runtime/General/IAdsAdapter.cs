using System;

namespace AdsManagement
{
    /// <summary>
    /// Base interface for all ad adapters.
    /// An adapter wraps a specific SDK (e.g., LevelPlay) to provide a unified contract
    /// for initializing, loading, and receiving ad lifecycle events.
    /// Each ad type (rewarded, interstitial, banner) extends this interface.
    /// </summary>
    public interface IAdsAdapter
    {
        bool IsReady { get; }

        void Initialize();
        void Load();

        event Action Loaded;
        event Action LoadFailed;
        event Action Displayed;
        event Action DisplayFailed;
    }
}
