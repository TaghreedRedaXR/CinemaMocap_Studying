using System;

namespace CinemaSuite.CinemaMocap.System.Core
{
    /// <summary>
    /// Enumeration of joint names that can encompass multiple input devices.
    /// </summary>
    [Serializable]
    public enum NUIJointType
    {
        Unspecified = -1,
        SpineBase = 0,
        SpineMid = 1,
        Neck = 2,
        Head = 3,
        ShoulderLeft = 4,
        ElbowLeft = 5,
        WristLeft = 6,
        HandLeft = 7,
        ShoulderRight = 8,
        ElbowRight = 9,
        WristRight = 10,
        HandRight = 11,
        HipLeft = 12,
        KneeLeft = 13,
        AnkleLeft = 14,
        FootLeft = 15,
        HipRight = 16,
        KneeRight = 17,
        AnkleRight = 18,
        FootRight = 19,
        SpineShoulder = 20,
        CollarLeft = 21,
        HandTipLeft = 22,
        ThumbLeft = 23,
        CollarRight = 24,
        HandTipRight = 25,
        ThumbRight = 26,
    }
}
