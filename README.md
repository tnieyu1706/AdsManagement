# Ads Management

A Unity package for managing ads using the **Adapter + Service** architecture.
Supports `async`/`await` (`Task`-based API) with built-in pacing and timeout.

## Requirements

- Unity **2021.3** or newer
- [com.unity.services.levelplay](https://docs.unity.com/ads/levelplay-sdk) (LevelPlay SDK)

## Installation

Add via Package Manager (UPM) with Git URL:

```
https://github.com/tnieyu1706/AdsManagement.git
```

Or add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tnieyu1706.adsmanagement": "https://github.com/tnieyu1706/AdsManagement.git"
  }
}
```

## Architecture

| Layer | Folder | Responsibility |
|---|---|---|
| **Adapter interfaces** | `Runtime/General/` | `IAdsAdapter` — unified contract for SDK initialization, loading, and lifecycle events |
| | `Runtime/BannerAds/` | `IBannerAdsAdapter` — adds show/hide for banner |
| | `Runtime/InterstitialAds/` | `IInterstitialAdsAdapter` — adds placement capping and close |
| | `Runtime/RewardAds/` | `IRewardAdsAdapter` — adds reward and close events |
| **Services** | `Runtime/BannerAds/` | `BannerAdsService` — banner with pacing, async load |
| | `Runtime/InterstitialAds/` | `InterstitialAdsService` — interstitial with async show + timeout |
| | `Runtime/RewardAds/` | `RewardedAdsService` — rewarded with reward tracking + timeout |
| **SDK adapters** | `Runtime/LevelPlayService/` | `LevelPlay*AdsAdapter` — LevelPlay SDK implementation of each adapter interface |

### Adapter vs Service

- **Adapter** wraps low-level SDK operations (initialize, load, show, hide) and exposes lifecycle events. You can write your own adapter for any SDK.
- **Service** consumes an adapter and adds business logic: state management (`AdState`), pacing, async load/show with timeout/cancellation, and event forwarding.

## Usage

### 1. Initialize LevelPlay

```csharp
using AdsManagement.LevelPlayService;

LevelPlayConfig.Initialize("YOUR_APP_KEY");
```

### 2. Create adapters and services

```csharp
var bannerAdapter = new LevelPlayBannerAdsAdapter("APP_KEY", "BANNER_UNIT_ID");
bannerAdapter.Initialize();

var banner = new BannerAdsService(bannerAdapter, pacingTime: 30f);
```

For interstitial/rewarded (with show timeout in ms):

```csharp
var interstitialAdapter = new LevelPlayInterstitialAdsAdapter("APP_KEY", "INTERSTITIAL_UNIT_ID");
interstitialAdapter.Initialize();

var interstitial = new InterstitialAdsService(interstitialAdapter, pacingTime: 10f, showTimeout: 30000);
```

### 3. Load & Show

**Banner** — persistent view, manual show/hide:

```csharp
await banner.LoadAd();
banner.ShowAd();
// later...
banner.HideAd();
```

**Interstitial** — async show, returns `AdResult`:

```csharp
await interstitial.LoadAd();
AdResult result = await interstitial.ShowAd("placement_name");
```

**Rewarded** — async show, returns `(AdResult, Reward?)`:

```csharp
await rewarded.LoadAd();
(AdResult result, Reward? reward) = await rewarded.ShowAd("placement_name", cancellationToken);
```

### AdResult

| Value | Description |
|---|---|
| `Success` | Ad finished successfully (reward granted for rewarded) |
| `Failed` | Ad display failed |
| `Interrupted` | User closed the ad before completion |
| `Timeout` | Ad timed out |

### AdState

| Value | Description |
|---|---|
| `AdapterNotReady` | Underlying adapter is not initialized |
| `Pacing` | Minimum interval has not elapsed since last show |
| `Playing` | An ad is currently being displayed |
| `Ready` | Ready to load or show |

## Custom SDK integration

Implement the adapter interfaces to support any ad SDK:

```csharp
public class MyCustomBannerAdapter : IBannerAdsAdapter
{
    public bool IsReady { get; }
    public void Initialize() { /* SDK init */ }
    public void Load() { /* load ad */ }
    public void Show() { /* show banner */ }
    public void Hide() { /* hide banner */ }

    public event Action Loaded;
    public event Action LoadFailed;
    public event Action Displayed;
    public event Action DisplayFailed;
}

var service = new BannerAdsService(new MyCustomBannerAdapter(), pacingTime: 0);
```

## License

[Unlicense](LICENSE)
