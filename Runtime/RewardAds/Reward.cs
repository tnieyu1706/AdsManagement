namespace AdsManagement
{
    /// <summary>Reward granted from a rewarded ad. Null when no reward is received.</summary>
    public struct Reward
    {
        public string Name { get; set; }
        public int Amount { get; set; }
    }
}
