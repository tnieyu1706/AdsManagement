using UnityEngine;

namespace AdsManagement
{
    [CreateAssetMenu(fileName = "AdsConfiguration", menuName = "AdsManagement/AdsConfiguration")]
    public class AdsConfiguration : ScriptableObject
    {
        public string appKey;
        [Space(4)] 
        public string bannerUnitId;
        public string interstitialUnitId;
        public string rewardedVideoUnitId;
    }
}