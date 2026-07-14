using System;

namespace AdsManagement
{
    /// <summary>Interface for banner ad adapters. Banners are persistent views with manual show/hide control.</summary>
    public interface IBannerAdsAdapter : IAdsAdapter
    {
        void Show();
        void Hide();
    }
}
