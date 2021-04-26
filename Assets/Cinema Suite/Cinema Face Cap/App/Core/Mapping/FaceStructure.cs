using System;
using System.Collections.Generic;

namespace CinemaSuite.CinemaFaceCap.App.Core.Mapping
{
    public class FaceStructure
    {
        private string assetName = "";
        private string orientationNodePath = "";
        private string facePath = "";
        private Dictionary<string, BlendShapeMetaData> blendShapeInfo = new Dictionary<string, BlendShapeMetaData>();

        public string AssetName
        {
            get
            {
                return assetName;
            }

            set
            {
                assetName = value;
            }
        }

        public string FacePath
        {
            get
            {
                return facePath;
            }

            set
            {
                facePath = value;
            }
        }

        public string OrientationNodePath
        {
            get
            {
                return orientationNodePath;
            }

            set
            {
                orientationNodePath = value;
            }
        }

        public FaceStructure(string assetName)
        {
            this.assetName = assetName;
        }

        public void Add(string name)
        {
            blendShapeInfo.Add(name, new BlendShapeMetaData() { include = false });
        }

        public void Add(string name, FaceShapeAnimations hint)
        {
            blendShapeInfo.Add(name, new BlendShapeMetaData() { hint = hint });
        }

        public void Add(string name, FaceShapeAnimations hint, float multiplier)
        {
            blendShapeInfo.Add(name, new BlendShapeMetaData() { hint = hint, mappingFunction = value => value * multiplier });
        }

        public void Add(string name, FaceShapeAnimations hint, Func<float, float> mappingFunction)
        {
            blendShapeInfo.Add(name, new BlendShapeMetaData() { hint = hint, mappingFunction = mappingFunction  });
        }

        public Dictionary<string, BlendShapeMetaData> BlendShapeInfo
        {
            get
            {
                return blendShapeInfo;
            }
        }

        public class BlendShapeMetaData
        {
            public bool include = true;

            /// <summary>
            /// A hint for what input should be mapped to this.
            /// </summary>
            public FaceShapeAnimations hint;

            /// <summary>
            /// The function for mapping from the input to the expected output range.
            /// </summary>
            public Func<float, float> mappingFunction = value => value;
        }
    }
}
