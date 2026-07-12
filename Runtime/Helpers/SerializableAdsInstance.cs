using System;
using UnityEngine;

namespace AdsManagement
{
    [Serializable]
    public class SerializableAdsInstance
    {
        [SerializeField] private AdsConfiguration adsConfiguration;
        private AdsInstance adsInstance;
        public AdsInstance Instance => adsInstance ??= new AdsInstance(adsConfiguration);
    }
}