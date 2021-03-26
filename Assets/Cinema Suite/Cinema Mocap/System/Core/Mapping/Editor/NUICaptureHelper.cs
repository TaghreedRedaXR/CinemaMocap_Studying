
using CinemaSuite.CinemaMocap.System.Core.Capture;
using System.Diagnostics;
using UnityEngine;
using Windows.Kinect;

namespace CinemaSuite.CinemaMocap.System.Core.Mapping
{
    public static class NUICaptureHelper
    {

        /// <summary>
        /// Maps a given ZigJointId to it's NUIJointType counterpart
        /// </summary>
        /// <param name="zigJointId">A ZigJointId</param>
        /// <returns>The corresponding NUIJointType</returns>
        public static NUIJointType ZigToNUIJointMapping(ZigJointId zigJointId)
        {
            switch (zigJointId)
            {
                case ZigJointId.Waist:
                    return NUIJointType.SpineBase;
                case ZigJointId.Torso:
                    return NUIJointType.SpineMid;
                case ZigJointId.Neck:
                    return NUIJointType.Neck;
                case ZigJointId.Head:
                    return NUIJointType.Head;
                case ZigJointId.RightHip:
                    return NUIJointType.HipRight;
                case ZigJointId.RightKnee:
                    return NUIJointType.KneeRight;
                case ZigJointId.RightAnkle:
                    return NUIJointType.AnkleRight;
                case ZigJointId.RightFoot:
                    return NUIJointType.FootRight;
                case ZigJointId.LeftHip:
                    return NUIJointType.HipLeft;
                case ZigJointId.LeftKnee:
                    return NUIJointType.KneeLeft;
                case ZigJointId.LeftAnkle:
                    return NUIJointType.AnkleLeft;
                case ZigJointId.LeftFoot:
                    return NUIJointType.FootLeft;
                case ZigJointId.RightShoulder:
                    return NUIJointType.ShoulderRight;
                case ZigJointId.RightElbow:
                    return NUIJointType.ElbowRight;
                case ZigJointId.RightWrist:
                    return NUIJointType.WristRight;
                case ZigJointId.RightHand:
                    return NUIJointType.HandRight;
                case ZigJointId.LeftShoulder:
                    return NUIJointType.ShoulderLeft;
                case ZigJointId.LeftElbow:
                    return NUIJointType.ElbowLeft;
                case ZigJointId.LeftWrist:
                    return NUIJointType.WristLeft;
                case ZigJointId.LeftHand:
                    return NUIJointType.HandLeft;
                default:
                    return NUIJointType.Unspecified;
            }
        }

        public static NUIJointType JointTypeToNUIJointTypeMapping(JointType jointType)
        {
            switch (jointType)
            {
                case JointType.SpineBase:
                    return NUIJointType.SpineBase;
                case JointType.SpineMid:
                    return NUIJointType.SpineMid;
                case JointType.SpineShoulder:
                    return NUIJointType.SpineShoulder;
                case JointType.Neck:
                    return NUIJointType.Neck;
                case JointType.Head:
                    return NUIJointType.Head;
                case JointType.HipRight:
                    return NUIJointType.HipRight;
                case JointType.KneeRight:
                    return NUIJointType.KneeRight;
                case JointType.AnkleRight:
                    return NUIJointType.AnkleRight;
                case JointType.FootRight:
                    return NUIJointType.FootRight;
                case JointType.HipLeft:
                    return NUIJointType.HipLeft;
                case JointType.KneeLeft:
                    return NUIJointType.KneeLeft;
                case JointType.AnkleLeft:
                    return NUIJointType.AnkleLeft;
                case JointType.FootLeft:
                    return NUIJointType.FootLeft;
                case JointType.ShoulderRight:
                    return NUIJointType.ShoulderRight;
                case JointType.ElbowRight:
                    return NUIJointType.ElbowRight;
                case JointType.WristRight:
                    return NUIJointType.WristRight;
                case JointType.HandRight:
                    return NUIJointType.HandRight;
                case JointType.ShoulderLeft:
                    return NUIJointType.ShoulderLeft;
                case JointType.ElbowLeft:
                    return NUIJointType.ElbowLeft;
                case JointType.WristLeft:
                    return NUIJointType.WristLeft;
                case JointType.HandLeft:
                    return NUIJointType.HandLeft;
                case JointType.HandTipLeft:
                    return NUIJointType.HandTipLeft;
                case JointType.ThumbLeft:
                    return NUIJointType.ThumbLeft;
                case JointType.HandTipRight:
                    return NUIJointType.HandTipRight;
                case JointType.ThumbRight:
                    return NUIJointType.ThumbRight;
                default:
                    {
                        return NUIJointType.Unspecified;
                    }
            }
        }

        public static NUISkeleton GetNUISkeleton(ZigInputJoint[] Skeleton)
        {
            NUISkeleton nuiSkeleton = new NUISkeleton();

            foreach(ZigInputJoint inputJoint in Skeleton)
            {
                NUIJointType jointType = ZigToNUIJointMapping(inputJoint.Id);

                // Convert position from mm to meters
                Vector3 position = inputJoint.Position / 1000f;

                NUIJoint joint = new NUIJoint(jointType, position, inputJoint.Rotation, inputJoint.Inferred);
                if (!nuiSkeleton.Joints.ContainsKey(jointType))
                {
                    nuiSkeleton.Joints.Add(jointType, joint);
                }
            }

            return nuiSkeleton;
        }

        public static NUISkeleton GetNUISkeleton(Body body)
        {
            var nuiSkeleton = new NUISkeleton();

            for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++)
            {
                NUIJointType jointType = JointTypeToNUIJointTypeMapping(jt);

                Vector3 position = new Vector3(body.Joints[jt].Position.X, body.Joints[jt].Position.Y, body.Joints[jt].Position.Z);

                // Reverse the Z
                position.z *= -1;

                Quaternion orientation = new Quaternion(body.JointOrientations[jt].Orientation.X, body.JointOrientations[jt].Orientation.Y, body.JointOrientations[jt].Orientation.Z, body.JointOrientations[jt].Orientation.W);
                
                NUIJoint joint = new NUIJoint(jointType, position, orientation, body.Joints[jt].TrackingState != Windows.Kinect.TrackingState.Tracked);
                if (!nuiSkeleton.Joints.ContainsKey(jointType))
                {
                    nuiSkeleton.Joints.Add(jointType, joint);
                }
            }

            return nuiSkeleton;
        }

        public static NUISkeleton GetNUISkeleton(SkeletonFrameData skeleton)
        {
            var nuiSkeleton = new NUISkeleton();

            for (NUIJointType jt = NUIJointType.SpineBase; jt <= NUIJointType.ThumbRight; jt++)
            {
                Vector3 position = new Vector3();
                Quaternion orientation = new Quaternion();
                var trackingState = CinemaSuite.CinemaMocap.System.Core.TrackingState.NotTracked;

                skeleton.GetJointData(jt, out position, out orientation, out trackingState);
                
                NUIJoint joint = new NUIJoint(jt, position, orientation, trackingState != CinemaSuite.CinemaMocap.System.Core.TrackingState.Tracked);
                if (!nuiSkeleton.Joints.ContainsKey(jt))
                {
                    nuiSkeleton.Joints.Add(jt, joint);
                }
            }

            return nuiSkeleton;
        }
    }
}
