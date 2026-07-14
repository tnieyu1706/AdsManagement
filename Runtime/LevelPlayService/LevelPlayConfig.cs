#if ADSMANAGEMENT_LEVELPLAY_SUPPORT
using System;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace AdsManagement.LevelPlayService
{
    /// <summary>
    /// Configuration and initialization for the LevelPlay SDK.
    /// </summary>
    public static class LevelPlayConfig
    {
        /// <summary>
        /// Indicates whether the LevelPlay SDK is initialized.
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Triggered when the LevelPlay SDK initialization completes. Provides the app key.
        /// </summary>
        public static event Action<string> OnInitialized;

        /// <summary>
        /// Initializes the LevelPlay SDK and validates integration.
        /// </summary>
        /// <param name="appKey">The LevelPlay application key.</param>
        public static void Initialize(string appKey) {
            Debug.Log($"[{nameof(LevelPlayConfig)}] Initializing...");

            LevelPlay.ValidateIntegration();
            LevelPlay.Init(appKey);
            OnInitialized?.Invoke(appKey);

            IsInitialized = true;

            Debug.Log($"[{nameof(LevelPlayConfig)}] Initialized.");
        }
    }
}
#endif