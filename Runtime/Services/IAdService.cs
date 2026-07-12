using System;
using System.Threading;
using System.Threading.Tasks;

namespace AdsManagement
{
    public interface IAdService
    {
        bool IsReady { get; }
        void LoadAd();
        Task<AdResult> ShowAsync(CancellationToken cancellationToken = default);
    }

    public abstract class BaseAdService : IAdService, IDisposable
    {
        protected readonly AdsSet Ads;
        public abstract bool IsReady { get; }

        protected BaseAdService(AdsSet ads) {
            Ads = ads;
        }

        public abstract void LoadAd();

        public abstract Task<AdResult> ShowAsync(CancellationToken cancellationToken = default);

        public abstract void Dispose();
    }
}