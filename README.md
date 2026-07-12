# Ads Management

A Unity package for managing and working with **LevelPlay** ads using `async`/`await` (`Task`-based API).

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

## Setup

### 1. Create AdsConfiguration

Right-click in Project window → **Create → AdsManagement → AdsConfiguration**

Fill in your LevelPlay App Key and Ad Unit IDs:

| Field | Description |
|---|---|
| `appKey` | LevelPlay App Key |
| `bannerUnitId` | Banner Ad Unit ID (optional) |
| `interstitialUnitId` | Interstitial Ad Unit ID (optional) |
| `rewardedVideoUnitId` | Rewarded Video Ad Unit ID (optional) |

### 2. Add AdsController

Attach `AdsController` to a GameObject in your initial scene and assign the `AdsConfiguration` asset.

## Usage

### AdResult

| Value | Description |
|---|---|
| `Completed` | Ad finished successfully (reward granted for rewarded) |
| `Closed` | User closed the ad before completion |
| `Failed` | Ad display failed |
| `NotReady` | Ad was not loaded/ready |
| `Timeout` | Ad timed out |
| `Cancelled` | Operation was cancelled via `CancellationToken` |

### Banner

```csharp
var banner = new BannerAdService(controller.Ads);

banner.LoadAd();
banner.Show();
banner.Hide();
```

### Interstitial

```csharp
var interstitial = new InterstitialAdService(controller.Ads);
interstitial.LoadAd();

AdResult result = await interstitial.ShowAsync();
```

### Rewarded

```csharp
var rewarded = new RewardedAdService(controller.Ads);
rewarded.LoadAd();

AdResult result = await rewarded.ShowAsync(cancellationToken);
```

All `ShowAsync()` methods support `CancellationToken` for timeout/cancellation.

## License

[Unlicense](LICENSE)
