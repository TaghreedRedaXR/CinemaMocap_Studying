using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Core.Capture
{
    [Serializable]
    public class FaceCapSession : ScriptableObject
    {
        [SerializeField]
        private FaceCapSessionMetaData metaData;

        [SerializeField]
        private List<FaceCapSessionKeyframe> captureData;

        public FaceCapSessionMetaData MetaData
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

        public List<FaceCapSessionKeyframe> CaptureData
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
    }
}