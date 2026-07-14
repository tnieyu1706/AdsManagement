namespace AdsManagement
{
    /// <summary>Describes the current state of an ad service — whether it is ready to show an ad.</summary>
    public enum AdState
    {
        AdapterNotReady,
        Pacing,
        Playing,
        Ready,
    }
}
