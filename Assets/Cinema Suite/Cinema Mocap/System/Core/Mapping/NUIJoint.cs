using System;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Mapping
{
    [Serializable]
    public class NUIJoint
    {
        // The type of joint this is.
        private NUIJointType jointType;

        // The translation of the joint
        [SerializeField]
        private Vector3 position;

        // The rotation of the joint.
        [SerializeField]
        private Quaternion rotation;

        public Vector3 directionToChild;

        // Is the joint inferred?
        private bool inferred;

        // What is the confidence of the inferred values.
        private float inferredQuality;

        // The local transformation matrix.
        public Matrix4x4 TransformationMatrix = new Matrix4x4();

        // The world transformation matrix.
        public Matrix4x4 WorldTransformationMatrix = Matrix4x4.identity;
        
        public NUIJoint(NUIJointType jointType)
        {
            this.jointType = jointType;
        }

        /// <summary>
        /// Create a representation of a Joint.
        /// </summary>
        /// <param name="jointType">The joint type.</param>
        /// <param name="position">The position from it's parent.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="inferred">Was the joint inferred?</param>
        public NUIJoint(NUIJointType jointType, Vector3 position, Quaternion rotation, bool inferred)
        {
            this.jointType = jointType;
            this.position = position;
            this.rotation = rotation;
            this.inferred = inferred;
        }

        public NUIJoint(NUIJointType jointType, Vector3 position, Quaternion rotation, float inferredQuality)
        {
            this.jointType = jointType;
            this.position = position;
            this.rotation = rotation;
            this.inferredQuality = inferredQuality;
        }

        /// <summary>
        /// Clones this object.
        /// </summary>
        /// <returns>The newly created clone.</returns>
        public NUIJoint Clone()
        {
            NUIJoint clone = new NUIJoint(this.jointType, this.position, this.rotation, this.inferred);
            clone.inferredQuality = this.inferredQuality;

            return clone;
        }

        public Vector3 Position
        {
            set
            {
                position = value;
            }
            get
            {
                return position;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        public bool Inferred
        {
            get
            {
                return inferred;
            }
        }

        public float InferredQuality
        {
            get
            {
                return inferredQuality;
            }
        }

        public NUIJointType JointType
        {
            get { return jointType; }
        }

        public override string ToString()
        {
            string message = position.ToString() + ", " + rotation.ToString() + ", " + directionToChild.ToString();
                return message;
        }
    }
}
