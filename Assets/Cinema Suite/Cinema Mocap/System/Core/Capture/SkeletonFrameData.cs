
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Capture
{
    /// <summary>
    /// A snapshot of all relevant skeleton data captured in one frame, used for motion capture.
    /// This is a generic data container for captured skeletons.
    /// It is important that this class is serializable in Unity! This means we can't use dictionaries.
    /// Because of this, be sure that the 4 lists of data are always in sync with each other.
    /// </summary>
    [Serializable]
    public class SkeletonFrameData
    {
        // Tracking Id
        [SerializeField]
        private ulong trackingId;

        // Is the skeleton tracked?
        [SerializeField]
        private bool isTracked;

        // Joint Types
        [SerializeField]
        private List<NUIJointType> jointTypes = new List<NUIJointType>();

        // Joint position data
        [SerializeField]
        private List<Vector3> jointPositions = new List<Vector3>();

        // Joint rotation data
        [SerializeField]
        private List<Quaternion> jointOrientations = new List<Quaternion>();

        // Inferrence data
        [SerializeField]
        private List<TrackingState> jointTracking = new List<TrackingState>();

        // Frame edge data
        [SerializeField]
        private FrameEdges clippedEdges;

        #region Kinect2

        // Left Hand
        private TrackingConfidence leftHandConfidence;
        private HandState leftHandState;

        // Right Hand
        private TrackingConfidence rightHandConfidence;
        private HandState rightHandState;
        #endregion

        public ulong TrackingId
        {
            get
            {
                return trackingId;
            }

            set
            {
                trackingId = value;
            }
        }

        public bool IsTracked
        {
            get
            {
                return isTracked;
            }

            set
            {
                isTracked = value;
            }
        }

        public FrameEdges ClippedEdges
        {
            get
            {
                return clippedEdges;
            }

            set
            {
                clippedEdges = value;
            }
        }

        public TrackingConfidence LeftHandConfidence
        {
            get
            {
                return leftHandConfidence;
            }

            set
            {
                leftHandConfidence = value;
            }
        }

        public HandState LeftHandState
        {
            get
            {
                return leftHandState;
            }

            set
            {
                leftHandState = value;
            }
        }

        public TrackingConfidence RightHandConfidence
        {
            get
            {
                return rightHandConfidence;
            }

            set
            {
                rightHandConfidence = value;
            }
        }

        public HandState RightHandState
        {
            get
            {
                return rightHandState;
            }

            set
            {
                rightHandState = value;
            }
        }

        public void AddJoint(NUIJointType jt, Vector3 position, Quaternion orientation, TrackingState trackingState)
        {
            jointTypes.Add(jt);
            jointPositions.Add(position);
            jointOrientations.Add(orientation);
            jointTracking.Add(trackingState);
        }

        public void GetJointData(NUIJointType jt, out Vector3 position, out Quaternion orientation, out TrackingState trackingState)
        {
            position = new Vector3();
            orientation = new Quaternion();
            trackingState = TrackingState.NotTracked;

            int index = -1;
            for (int i = 0; i < jointTypes.Count; i++)
            {
                if (jointTypes[i] == jt)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                position = jointPositions[index];
                orientation = jointOrientations[index];
                trackingState = jointTracking[index];
            }
        }
    }
}