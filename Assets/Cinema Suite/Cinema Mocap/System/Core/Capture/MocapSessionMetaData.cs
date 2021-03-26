using System;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Capture
{
    [Serializable]
    public class MocapSessionMetaData
    {
        [SerializeField]
        private SupportedDevice captureDevice;

        [SerializeField]
        private bool isHandDataAvailable;

        [SerializeField]
        private bool isFrameEdgeDataAvailable;

        [SerializeField]
        private InputSkeletonType inputSkeletonType;

        public InputSkeletonType InputSkeletonType
        {
            get
            {
                return inputSkeletonType;
            }

            set
            {
                inputSkeletonType = value;
            }
        }

        public bool IsHandDataAvailable
        {
            get
            {
                return isHandDataAvailable;
            }

            set
            {
                isHandDataAvailable = value;
            }
        }

        public bool IsFrameEdgeDataAvailable
        {
            get
            {
                return isFrameEdgeDataAvailable;
            }

            set
            {
                isFrameEdgeDataAvailable = value;
            }
        }

        public SupportedDevice CaptureDevice
        {
            get
            {
                return captureDevice;
            }

            set
            {
                captureDevice = value;
            }
        }
    }
}
