using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Collada
{
    /// <summary>
    /// Joint data as read from a Collada file.
    /// </summary>
    public class ColladaJointData
    {
        private string id;
        private Matrix4x4 LHStransformationMatrix = new Matrix4x4();
        private Matrix4x4 LHSworldTransformationMatrix = Matrix4x4.identity;
        private Vector3 translation;
        private Quaternion rotation;
        private Vector3 rotationVector;

        public ColladaJointData(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// Rotation Vector of the joint
        /// </summary>
        public Vector3 RotationVector
        {
            get { return rotationVector; }
            set { rotationVector = value; }
        }

        /// <summary>
        /// Translation of the Joint
        /// </summary>
        public Vector3 Translation
        {
            get { return translation; }
            set { translation = value; }
        }

        /// <summary>
        /// Rotation of the Joint
        /// </summary>
        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// LHS Transformation Matrix of the Joint Data
        /// </summary>
        public Matrix4x4 LHSTransformationMatrix
        {
            get { return LHStransformationMatrix; }
            set { LHStransformationMatrix = value; }
        }

        /// <summary>
        /// LHS World Transformation Matrix of the Joint Data
        /// </summary>
        public Matrix4x4 LHSWorldTransformationMatrix
        {
            get { return LHSworldTransformationMatrix; }
            set { LHSworldTransformationMatrix = value; }
        }

        /// <summary>
        /// The Id of this joint.
        /// </summary>
        public string Id
        {
            get { return id; }
        }

        /// <summary>
        /// Provides a summary of this object, including joint Id and transformation matrix.
        /// </summary>
        /// <returns>Summary of object</returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Id, LHSTransformationMatrix.ToString());
        }

    }
}