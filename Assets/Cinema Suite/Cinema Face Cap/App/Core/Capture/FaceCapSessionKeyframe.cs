
using System;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Core.Capture
{
    [Serializable]
    public class FaceCapSessionKeyframe
    {
        [SerializeField]
        private FaceCapFrameData frameData;

        [SerializeField]
        private int elapsedMilliseconds;

        public int ElapsedMilliseconds
        {
            get
            {
                return elapsedMilliseconds;
            }
        }

        public FaceCapFrameData FrameData
        {
            get
            {
                return frameData;
            }
        }

        public FaceCapSessionKeyframe(FaceCapFrameData frameData, int elapsedMilliseconds)
        {
            this.frameData = frameData;
            this.elapsedMilliseconds = elapsedMilliseconds;
        }
    }
}