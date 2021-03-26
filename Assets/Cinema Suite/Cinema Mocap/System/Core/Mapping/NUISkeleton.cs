
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Mapping
{
    /// <summary>
    /// A Skeleton that maintains structure info and joint data.
    /// </summary>
    [Serializable]
    public class NUISkeleton
    {
        // The structure of this skeleton.
        public SkeletonStructure Structure;

        // The Joints in this skeleton.
        [SerializeField]
        public Dictionary<NUIJointType, NUIJoint> Joints = new Dictionary<NUIJointType, NUIJoint>();

        // The direction of right from chest and spine.
        public Vector3 SpineBaseRight = Vector3.right;
        public Vector3 ChestRight = Vector3.right;

        public NUISkeleton()
        { }

        public NUISkeleton(SkeletonStructure structure)
        {
            this.Structure = structure;
        }

        /// <summary>
        /// Make a clone of this skeleton. Creating a deep copy of the Joints
        /// and shallow copy of the Structure.
        /// </summary>
        /// <returns>A new NUISkeleton.</returns>
        public NUISkeleton Clone()
        {
            NUISkeleton clone = new NUISkeleton();
            clone.Structure = this.Structure;

            foreach(var joint in this.Joints)
            {
                NUIJoint jointClone = joint.Value.Clone();
                clone.Joints.Add(jointClone.JointType, jointClone);
            }

            return clone;
        }

        public override string ToString()
        {
            string message = "Spine Base Right, " + SpineBaseRight.ToString() + "\n";
            message += "Chest Right, " + ChestRight.ToString() + "\n";

            message += "Joint Type, Position, Rotation, Direction to Child \n";
            foreach (NUIJointType jointType in Enum.GetValues(typeof(NUIJointType)))
            {
                if (Structure.IsJointInStructure(jointType))
                {
                    message += jointType.ToString() + ", " + Joints[jointType].ToString() + "\n";
                }
            }
            
            return message;
        }
    }
}
