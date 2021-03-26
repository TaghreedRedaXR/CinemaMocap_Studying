
using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline.Output;
using System;
using System.Linq;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline.Mapping
{
    [MappingProfileAttribute("Kinect -> 20 Joint", InputSkeletonType.Kinect1_20Joint, typeof(Standard20Joint))]
    public class Kinect1To20JointColladaMapper : MappingProfile
    {
        public override NUISkeleton MapSkeleton(NUISkeleton inputSkeleton)
        {
            NUISkeleton outputSkeleton = new NUISkeleton(OutputStructure.Structure);
            NUIJoint parentJoint = OutputStructure.Joints[OutputStructure.Structure.RootNode];
            outputSkeleton.Joints.Add(OutputStructure.Structure.RootNode, parentJoint);

            foreach (NUIJointType jointType in Enum.GetValues(typeof(NUIJointType)))
            {
                if (isJointMasked(jointType))
                {
                    if (OutputStructure.Joints.ContainsKey(jointType))
                    {
                        NUIJoint maskedJoint = OutputStructure.Joints[jointType];
                        outputSkeleton.Joints.Add(jointType, maskedJoint);
                        continue;
                    }
                }
                if (OutputStructure.Structure.IsJointInStructure(jointType))
                {
                    if (!OutputStructure.Structure.IsJointAnExtremity(jointType))
                    {
                        //if (jointType == NUIJointType.SpineBase)
                        {
                            // Build the skeleton for this current joint.
                            RotateJoint(jointType, inputSkeleton, outputSkeleton);
                        }
                    }
                    else
                    {
                        // If extremity, just copy from OutputStructure.
                        NUIJoint extremity = OutputStructure.Joints[jointType];
                        outputSkeleton.Joints.Add(jointType, extremity);
                    }
                }
            }
            return outputSkeleton;
        }

        public override NUIHumanoidAnimation MapAnimation(NUIHumanoidAnimation animation)
        {
            NUIHumanoidAnimation mappedAnimation = new NUIHumanoidAnimation();

            foreach (NUIAnimationKeyframe kf in animation.Keyframes)
            {
                Vector3 position = GetHipPosition(kf.Skeleton);

                NUISkeleton mappedSkeleton = MapSkeleton(kf.Skeleton);

                mappedSkeleton.Joints[NUIJointType.SpineBase].Position = position;
                NUIAnimationKeyframe newKF = new NUIAnimationKeyframe(mappedSkeleton, kf.ElapsedTime);

                mappedAnimation.AddKeyframe(newKF);
            }

            return mappedAnimation;
        }

        private void RotateJoint(NUIJointType jointType, NUISkeleton skeleton, NUISkeleton rig)
        {
            // Get Info from the OutputStructure
            NUIJointType parentJointType = rig.Structure.GetParentJoint(jointType);
            NUIJointType childJointType = rig.Structure.GetChildJoint(jointType);
            Quaternion jointLocalRotation = OutputStructure.Joints[jointType].Rotation;
            Vector3 direction = OutputStructure.Joints[jointType].directionToChild;
            Matrix4x4 matrix = rig.Joints[parentJointType].WorldTransformationMatrix;

            // Get the target direction based on the captured skeleton.
            Vector3 target = new Vector3();
            if (rig.Structure.IsJointParentToMany(jointType))
            {
                if (jointType == NUIJointType.SpineBase)
                {
                    Vector3 worldDirection = matrix.inverse.MultiplyVector(direction);
                    bool hipDirectionInverted = (worldDirection.y > 0);

                    target = ((skeleton.Joints[NUIJointType.HipLeft].Position + skeleton.Joints[NUIJointType.HipRight].Position) / 2F) - skeleton.Joints[NUIJointType.SpineBase].Position;
                    if (hipDirectionInverted)
                    {
                        target.y *= -1;
                    }
                }
            }
            else
            {
                target = skeleton.Joints[childJointType].Position - skeleton.Joints[jointType].Position;
            }

            // Get the parent's matrix data
            NUIJoint outputJoint = OutputStructure.Joints[jointType];
            matrix *= outputJoint.TransformationMatrix;

            // Transform the target from capture space to skeleton space.
            target = matrix.inverse.MultiplyVector(target);

            // Obtain the rotation from the original joint direction to the target direction.
            Quaternion quat = Quaternion.FromToRotation(direction, target);

            //if (jointType == NUIJointType.SpineMid)
            //{
            //    direction = OutputStructure.ChestRight;

            //    target = skeleton.Joints[NUIJointType.ShoulderRight].Position - skeleton.Joints[NUIJointType.ShoulderLeft].Position;

            //    target = matrix.inverse.MultiplyVector(target);
            //    target -= Vector3.Project(target, OutputStructure.Joints[jointType].directionToChild);

            //    quat *= Quaternion.FromToRotation(direction, target);
            //}

            if (jointType == NUIJointType.SpineBase)
            {
                direction = OutputStructure.SpineBaseRight;

                target = skeleton.Joints[NUIJointType.HipRight].Position - skeleton.Joints[NUIJointType.HipLeft].Position;
                target = matrix.inverse.MultiplyVector(target);
                target -= Vector3.Project(target, OutputStructure.Joints[jointType].directionToChild);

                quat *= Quaternion.FromToRotation(direction, target);
            }

            // Knee correction TODO: Make this optional
            if (jointType == NUIJointType.KneeLeft || jointType == NUIJointType.KneeRight ||
                jointType == NUIJointType.HipLeft || jointType == NUIJointType.HipRight)
            {
                quat *= Quaternion.Euler(0, 0, -5f);
            }
            if(jointType == NUIJointType.AnkleLeft || jointType == NUIJointType.AnkleRight)
            {
                quat *= Quaternion.Euler(0, 0, -30f);
            }

            jointLocalRotation *= quat;

            if (jointType == NUIJointType.SpineBase)
            {
                //jointLocalRotation = Quaternion.identity;
            }

            // Update the rig
            NUIJoint newJoint = new NUIJoint(jointType);
            newJoint.Position = new Vector3(-outputJoint.Position.x, outputJoint.Position.y, outputJoint.Position.z);
            newJoint.Rotation = jointLocalRotation;
            newJoint.TransformationMatrix = Matrix4x4.TRS(newJoint.Position, newJoint.Rotation, Vector3.one);
            newJoint.WorldTransformationMatrix = rig.Joints[parentJointType].WorldTransformationMatrix * newJoint.TransformationMatrix;

            if (!rig.Joints.ContainsKey(jointType))
            {
                rig.Joints.Add(jointType, newJoint);
            }
            else
            {
                rig.Joints[jointType] = newJoint;
            }
        }

        public override Vector3 GetHipPosition(NUISkeleton skeleton)
        {
            if (!skeleton.Joints.Keys.Contains<NUIJointType>(NUIJointType.SpineBase))
            {
                Debug.Log("skeleton has problem: " + skeleton.Joints.Count);
            }
            NUIJoint joint = skeleton.Joints[NUIJointType.SpineBase];
            Vector3 positionInMeters = joint.Position;
            return positionInMeters;
        }
    }
}