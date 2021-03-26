using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Mapping
{
    /// <summary>
    /// Wrapper class for Captured joint data and elapsed frame times.
    /// </summary>
    [Serializable]
    public class NUIHumanoidAnimation
    {
        [SerializeField]
        private List<NUIAnimationKeyframe> keyframes = new List<NUIAnimationKeyframe>();

        /// <summary>
        /// Returns the recorded keyframes
        /// </summary>
        public List<NUIAnimationKeyframe> Keyframes
        {
            get
            {
                return keyframes;
            }
        }

        /// <summary>
        /// Add a frame of capture data.
        /// </summary>
        /// <param name="skeleton">The current frame's skeleton capture data.</param>
        /// <param name="elapsedMilliseconds">The elapsed time since the last frame.</param>
        public void AddKeyframe(NUISkeleton skeleton, long elapsedMilliseconds)
        {
            float timeElapsed = elapsedMilliseconds / 1000f;
            keyframes.Add(new NUIAnimationKeyframe(skeleton, timeElapsed));
        }

        public void AddKeyframe(NUIAnimationKeyframe keyframe)
        {
            keyframes.Add(keyframe);
        }

        /// <summary>
        /// Create a new Animation with a constrained fps based on this animation.
        /// </summary>
        /// <param name="fps">The new frame rate</param>
        /// <returns>A new animation constrained by a given frame rate.</returns>
        public NUIHumanoidAnimation ConstrainFramerate(int fps)
        {
            NUIHumanoidAnimation animation = new NUIHumanoidAnimation();

            NUIAnimationKeyframe finalFrame = this.Keyframes[this.Keyframes.Count - 1];
            float finalTime = finalFrame.ElapsedTime;
            float currentTime = 0f;
            float timeIncrements = (1 / (float)fps);

            while (currentTime < finalTime)
            {
                // Find one or two keyframes that straddle the currentTime
                NUIAnimationKeyframe keyframe1 = null;
                NUIAnimationKeyframe keyframe2 = null;

                for (int i = 0; i < this.Keyframes.Count - 1; i++)
                {
                    keyframe1 = this.Keyframes[i];
                    keyframe2 = this.Keyframes[i + 1];

                    if (keyframe1.ElapsedTime <= currentTime && currentTime < keyframe2.ElapsedTime)
                    {
                        break;
                    }
                }

                // Determine the joint values at the current Time.
                if (currentTime == keyframe1.ElapsedTime)
                {
                    animation.AddKeyframe(keyframe1.Skeleton, (long)(currentTime * 1000));
                }
                else if (keyframe1.ElapsedTime <= currentTime && currentTime < keyframe2.ElapsedTime)
                {
                    NUISkeleton tweenedSkeleton = new NUISkeleton();

                    foreach (NUIJointType jointType in keyframe1.Skeleton.Joints.Keys)
                    {
                        float t = (currentTime - keyframe1.ElapsedTime) / (keyframe2.ElapsedTime - keyframe1.ElapsedTime);
                        Vector3 position = Vector3.Lerp(keyframe1.Skeleton.Joints[jointType].Position, keyframe2.Skeleton.Joints[jointType].Position, t);

                        Quaternion rotation = Quaternion.identity;
                        if (keyframe1.Skeleton.Joints[jointType].Rotation.w != 0 && keyframe2.Skeleton.Joints[jointType].Rotation.w != 0)
                        {
                            rotation = Quaternion.Lerp(keyframe1.Skeleton.Joints[jointType].Rotation, keyframe2.Skeleton.Joints[jointType].Rotation, t);
                        }
                        NUIJoint nuiJoint = new NUIJoint(jointType, position, rotation, false);
                        tweenedSkeleton.Joints.Add(jointType, nuiJoint);
                    }

                    animation.AddKeyframe(tweenedSkeleton, (long)(currentTime * 1000));
                }

                currentTime += timeIncrements;
            }

            return animation;
        }
    }
}