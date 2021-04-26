using System;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Core.Capture
{
    [Serializable]
    public class FaceCapSessionMetaData
    {
        [SerializeField]
        public SupportedDevice captureDevice;
    }
}
