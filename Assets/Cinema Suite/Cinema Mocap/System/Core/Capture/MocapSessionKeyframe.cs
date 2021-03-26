using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Capture
{
    [Serializable]
    public class MocapSessionKeyframe
    {
        [SerializeField]
        private SkeletonFrameData skeleton;

        [SerializeField]
        private int elapsedMilliseconds;

        public int ElapsedMilliseconds
        {
            get
            {
                return elapsedMilliseconds;
            }

        }

        public SkeletonFrameData Skeleton
        {
            get
            {
                return skeleton;
            }
        }

        public MocapSessionKeyframe(SkeletonFrameData frameData, int elapsedMilliseconds)
        {
            this.skeleton = frameData;
            this.elapsedMilliseconds = elapsedMilliseconds;
        }

    }
}
