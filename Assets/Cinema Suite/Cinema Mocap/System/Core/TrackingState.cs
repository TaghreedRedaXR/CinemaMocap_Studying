using System;

namespace CinemaSuite.CinemaMocap.System.Core
{
    [Serializable]
    public enum TrackingState : int
    {
        NotTracked = 0,
        Inferred = 1,
        Tracked =2,
    }
}
