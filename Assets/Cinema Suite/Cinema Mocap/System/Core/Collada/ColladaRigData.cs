using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Collada
{
    /// <summary>
    /// A representation of a COLLADA file's rig data. This class contains a collection of Joint data.
    /// </summary>
    public class ColladaRigData
    {
        private Dictionary<string, ColladaJointData> jointData = new Dictionary<string, ColladaJointData>();
        private Dictionary<string, string> jointHierarchy = new Dictionary<string, string>();

        /// <summary>
        /// Add a joint to the rig. This should be used to add the root node.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="joint"></param>
        public void Add(string id, ColladaJointData joint)
        {
            jointData.Add(id, joint);
        }

        /// <summary>
        /// Add a child node to the rig and specify the parent id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <param name="joint"></param>
        public void Add(string id, string parentId, ColladaJointData joint)
        {
            jointData.Add(id, joint);
            jointHierarchy.Add(id, parentId);
        }

        /// <summary>
        /// Get a joint from the collection, based on the joint Id
        /// </summary>
        /// <param name="jointId">The Id of the joint to be retrieved.</param>
        /// <returns>Returns the joint.</returns>
        public ColladaJointData GetJoint(string jointId)
        {
            return jointData[jointId];
        }

        /// <summary>
        /// Get the given joint's parent.
        /// </summary>
        /// <param name="jointId">The joint for which you want to retrieve the corresponding parent.</param>
        /// <returns>The parent joint</returns>
        public ColladaJointData GetJointParent(string jointId)
        {
            return GetJoint(jointHierarchy[jointId]);
        }

        /// <summary>
        /// The amount of joints in this rig.
        /// </summary>
        public int JointCount
        {
            get
            {
                return jointData.Count;
            }
        }

        public Dictionary<string, ColladaJointData> JointData
        {
            get { return jointData; }
        }

        /// <summary>
        /// Returns the contents of this collection.
        /// </summary>
        /// <returns>String containing the contents of each joint in the collection.</returns>
        public override string ToString()
        {
            string output = string.Empty;

            foreach (ColladaJointData joint in jointData.Values)
            {
                output = string.Format("{0} \n {1}", output, joint.ToString());
            }
            return output;
        }

        public List<string> GetJointNames()
        {
            var names = new List<string>(jointData.Keys);
            return names;
        }
    }

}