using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Capture
{
    [Serializable]
    public class MocapSession : ScriptableObject
    {
        [SerializeField]
        private MocapSessionMetaData metaData;

        [SerializeField]
        private List<MocapSessionKeyframe> captureData;

        public MocapSessionMetaData MetaData
        {
            get
            {
                return metaData;
            }

            set
            {
                metaData = value;
            }
        }

        public List<MocapSessionKeyframe> CaptureData
        {
            get
            {
                return captureData;
            }

            set
            {
                captureData = value;
            }
        }

        public MocapSession()
        {
        }
    }
}