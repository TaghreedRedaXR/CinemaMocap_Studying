using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline.Filters
{
    [NameAttribute("Mirror")]
    [MocapFilterAttribute(true)]
    [Ordinal(0)]
    public class MirrorFilter : MocapFilter
    {
        public MirrorFilter() { }

        public override NUISkeleton Filter(NUISkeleton input)
        {
            NUISkeleton output = input.Clone();

            swapJoints(output, NUIJointType.HipLeft, NUIJointType.HipRight);
            swapJoints(output, NUIJointType.KneeLeft, NUIJointType.KneeRight);
            swapJoints(output, NUIJointType.AnkleLeft, NUIJointType.AnkleRight);
            swapJoints(output, NUIJointType.FootLeft, NUIJointType.FootRight);

            swapJoints(output, NUIJointType.ShoulderLeft, NUIJointType.ShoulderRight);
            swapJoints(output, NUIJointType.ElbowLeft, NUIJointType.ElbowRight);
            swapJoints(output, NUIJointType.WristLeft, NUIJointType.WristRight);
            swapJoints(output, NUIJointType.HandLeft, NUIJointType.HandRight);

            foreach (NUIJointType jointType in output.Joints.Keys)
            {
                output.Joints[jointType].Position = new Vector3(-output.Joints[jointType].Position.x, output.Joints[jointType].Position.y, output.Joints[jointType].Position.z);
            }

            return output;
        }

        private void swapJoints(NUISkeleton skeleton, NUIJointType left, NUIJointType right)
        {
            NUIJoint leftJoint = new NUIJoint(right, skeleton.Joints[left].Position, skeleton.Joints[left].Rotation, skeleton.Joints[left].Inferred);
            NUIJoint rightJoint = new NUIJoint(left, skeleton.Joints[right].Position, skeleton.Joints[right].Rotation, skeleton.Joints[right].Inferred);

            skeleton.Joints[right] = leftJoint;
            skeleton.Joints[left] = rightJoint;
        }
    }
}
