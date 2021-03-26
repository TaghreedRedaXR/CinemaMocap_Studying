
using System;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Mapping
{
    /// <summary>
    /// A single frame of a humanoid animation captured by a NUI device.
    /// </summary>
    [Serializable]
    public class NUIAnimationKeyframe
    {
        [SerializeField]
        private NUISkeleton skeleton;

        [SerializeField]
        private float elapsedTime;

        /// <summary>
        /// Create an animation keyframe.
        /// </summary>
        /// <param name="skeleton">The skeleton data that was captured.</param>
        /// <param name="elapsedTime">The elapsed time.</param>
        public NUIAnimationKeyframe(NUISkeleton skeleton, float elapsedTime)
        {
            this.skeleton = skeleton;
            this.elapsedTime = elapsedTime;
        }

        /// <summary>
        /// The captured skeleton data of this keyframe.
        /// </summary>
        public NUISkeleton Skeleton
        {
            get
            {
                return this.skeleton;
            }
        }

        /// <summary>
        /// The elapsed time of this keyframe.
        /// </summary>
        public float ElapsedTime
        {
            get
            {
                return this.elapsedTime;
            }
        }
    }
}
