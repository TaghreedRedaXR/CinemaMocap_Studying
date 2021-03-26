using System.Collections.Generic;

namespace CinemaSuite.CinemaMocap.System.Core.Collada
{
    /// <summary>
    /// A container for animation data, to be written to a COLLADA file.
    /// </summary>
    public class ColladaAnimationData
    {
        public List<string> joints = new List<string>();
        public Dictionary<string, string> jointValues = new Dictionary<string, string>();

        public Dictionary<string, string> jointTranslateX = new Dictionary<string, string>();
        public Dictionary<string, string> jointTranslateY = new Dictionary<string, string>();
        public Dictionary<string, string> jointTranslateZ = new Dictionary<string, string>();

        public Dictionary<string, string> jointRotateX = new Dictionary<string, string>();
        public Dictionary<string, string> jointRotateY = new Dictionary<string, string>();
        public Dictionary<string, string> jointRotateZ = new Dictionary<string, string>();

        public List<float> frameTimelapse = new List<float>();

        public ColladaAnimationData(ColladaRigData rig)
        {
            foreach (KeyValuePair<string, ColladaJointData> jointData in rig.JointData)
            {
                this.joints.Add(jointData.Key);
                this.jointTranslateX.Add(jointData.Key, string.Empty);
                this.jointTranslateY.Add(jointData.Key, string.Empty);
                this.jointTranslateZ.Add(jointData.Key, string.Empty);
                this.jointRotateX.Add(jointData.Key, string.Empty);
                this.jointRotateY.Add(jointData.Key, string.Empty);
                this.jointRotateZ.Add(jointData.Key, string.Empty);
                this.jointValues.Add(jointData.Key, string.Empty);
            }
        }
    }
}