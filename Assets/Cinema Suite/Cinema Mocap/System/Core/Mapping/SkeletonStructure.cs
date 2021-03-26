using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Mapping
{
    /// <summary>
    /// Define the Structure of a Skeleton.
    /// </summary>
    public class SkeletonStructure
    {
        private NUIJointType rootNode;
        private Dictionary<NUIJointType, NUIJointType> structure = new Dictionary<NUIJointType, NUIJointType>();
        private Dictionary<NUIJointType, NUIJointType> childLookup = new Dictionary<NUIJointType, NUIJointType>();
        private Dictionary<NUIJointType, bool> extremity = new Dictionary<NUIJointType, bool>();
        private Dictionary<NUIJointType, bool> multiChildren = new Dictionary<NUIJointType, bool>();

        /// <summary>
        /// Set the Root node of the structure.
        /// </summary>
        /// <param name="rootJoint">The root joint of the skeleton.</param>
        /// <returns>true if added successfully.</returns>
        public bool SetRootJoint(NUIJointType rootJoint)
        {
            if (structure.Count == 0)
            {
                rootNode = rootJoint;
                structure.Add(rootJoint, rootJoint);
                extremity.Add(rootJoint, true);
                multiChildren.Add(rootJoint, false);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a Bone to the skeleton structure. Structure must be built incrementally.
        /// </summary>
        /// <param name="child">The child joint.</param>
        /// <param name="parent">The parent joint.</param>
        /// <returns>true if added successfully.</returns>
        public bool AddBone(NUIJointType child, NUIJointType parent)
        {
            if (structure.Count == 0)
            {
                SetRootJoint(parent);
            }

            if(!structure.ContainsKey(parent))
            {
                return false;
            }

            // Add to the structure
            structure.Add(child, parent);
            

            // Update the extremity lookup table
            extremity.Add(child, true);
            bool parentHasChild = !extremity[parent];
            extremity[parent] = false;

            // Update the multiChildren lookup table
            multiChildren.Add(child, false);
            if(parentHasChild)
            {
                multiChildren[parent] = true;
                childLookup[parent] = NUIJointType.Unspecified;
            }
            else
            {
                childLookup.Add(parent, child);
            }

            return true;
        }

        /// <summary>
        /// Is a joint in this specific skeletal structure?
        /// </summary>
        /// <param name="joint">The joint to check.</param>
        /// <returns>Return true if the joint is in the structure.</returns>
        public bool IsJointInStructure(NUIJointType joint)
        {
            return structure.ContainsKey(joint) || structure.ContainsValue(joint);
        }

        /// <summary>
        /// Is this joint an extremity in the skeletal structure?
        /// </summary>
        /// <param name="joint">The joint to check for.</param>
        /// <returns>true if the joint is an extremity in this structure.</returns>
        public bool IsJointAnExtremity(NUIJointType joint)
        {
            return extremity[joint];
        }

        /// <summary>
        /// Is a joint a parent to more than one other joint?
        /// </summary>
        /// <param name="joint">The joint to check for.</param>
        /// <returns>returns true if the joint is parent to more than one joint in this skeletal structure.</returns>
        public bool IsJointParentToMany(NUIJointType joint)
        {
            return multiChildren[joint];
        }

        /// <summary>
        /// Get the root node type of this structure.
        /// </summary>
        public NUIJointType RootNode
        {
            get
            {
                return rootNode;
            }
        }

        public NUIJointType GetParentJoint(NUIJointType childType)
        {
            return structure[childType];
        }

        public NUIJointType GetChildJoint(NUIJointType parentType)
        {
            return childLookup[parentType];
        }

        public List<NUIJointType> OrderedJointTypes
        {
            get
            {
                return new List<NUIJointType>(extremity.Keys);
            }
        }
    }
}
