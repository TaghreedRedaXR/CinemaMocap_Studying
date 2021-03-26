using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Capture;
using UnityEngine;


namespace CinemaSuite.CinemaMocap.System.Behaviours
{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteInEditMode]
    public class SkeletonRenderer : MonoBehaviour
    {

        LineRenderer lineRenderer;
        private const float LINE_WIDTH = 0.03f;
        private InputSkeletonType skeletonType = InputSkeletonType.None;

        #region Skeleton Joint Orders

        private readonly NUIJointType[] Skeleton20JointOrder =
        {
        NUIJointType.FootLeft,
        NUIJointType.AnkleLeft,
        NUIJointType.KneeLeft,
        NUIJointType.HipLeft,
        NUIJointType.SpineBase,
        NUIJointType.HipRight,
        NUIJointType.KneeRight,
        NUIJointType.AnkleRight,
        NUIJointType.FootRight,
        NUIJointType.AnkleRight,
        NUIJointType.KneeRight,
        NUIJointType.HipRight,
        NUIJointType.SpineBase,
        NUIJointType.SpineMid,
        NUIJointType.Neck,
        NUIJointType.ShoulderLeft,
        NUIJointType.ElbowLeft,
        NUIJointType.WristLeft,
        NUIJointType.HandLeft,
        NUIJointType.WristLeft,
        NUIJointType.ElbowLeft,
        NUIJointType.ShoulderLeft,
        NUIJointType.Neck,
        NUIJointType.Head,
        NUIJointType.Neck,
        NUIJointType.ShoulderRight,
        NUIJointType.ElbowRight,
        NUIJointType.WristRight,
        NUIJointType.HandRight
    };

        private readonly NUIJointType[] Skeleton25JointOrder =
        {
        NUIJointType.FootLeft,
        NUIJointType.AnkleLeft,
        NUIJointType.KneeLeft,
        NUIJointType.HipLeft,
        NUIJointType.SpineBase,
        NUIJointType.HipRight,
        NUIJointType.KneeRight,
        NUIJointType.AnkleRight,
        NUIJointType.FootRight,
        NUIJointType.AnkleRight,
        NUIJointType.KneeRight,
        NUIJointType.HipRight,
        NUIJointType.SpineBase,
        NUIJointType.SpineMid,
        NUIJointType.SpineShoulder,
        NUIJointType.ShoulderLeft,
        NUIJointType.ElbowLeft,
        NUIJointType.WristLeft,
        NUIJointType.HandLeft,
        NUIJointType.HandTipLeft,
        NUIJointType.HandLeft,
        NUIJointType.ThumbLeft,
        NUIJointType.HandLeft,
        NUIJointType.WristLeft,
        NUIJointType.ElbowLeft,
        NUIJointType.ShoulderLeft,
        NUIJointType.SpineShoulder,
        NUIJointType.Neck,
        NUIJointType.Head,
        NUIJointType.Neck,
        NUIJointType.SpineShoulder,
        NUIJointType.ShoulderRight,
        NUIJointType.ElbowRight,
        NUIJointType.WristRight,
        NUIJointType.HandRight,
        NUIJointType.HandTipRight,
        NUIJointType.HandRight,
        NUIJointType.ThumbRight,
    };

        #endregion

        void Awake()
        {
            // Get/Create the LineRenderer component
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                Debug.Log("LineRenderer not found, adding it...");
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                Debug.Log("LineRenderer set.");
            }

            // LineRenderer Settings
#if UNITY_5_5_OR_NEWER
            lineRenderer.startWidth = LINE_WIDTH;
            lineRenderer.endWidth = LINE_WIDTH;
#else
            lineRenderer.SetWidth(LINE_WIDTH, LINE_WIDTH);
#endif
            lineRenderer.useWorldSpace = false;
        }

        public void UpdateSkeleton(SkeletonFrameData skeletonFrameData, InputSkeletonType inputSkeletonType)
        {
            if (lineRenderer == null)
            {
                Debug.Log("LineRenderer STILL not found!");
            }

            NUIJointType[] currentOrder = new NUIJointType[0];

            if (inputSkeletonType != skeletonType)
            {
                switch (inputSkeletonType)
                {
                    case InputSkeletonType.Kinect1_20Joint:
#if UNITY_5_5_OR_NEWER
                        lineRenderer.positionCount = 29;
#else
                        lineRenderer.SetVertexCount(29);
#endif
                        currentOrder = Skeleton20JointOrder;
                        break;
                    case InputSkeletonType.Kinect2_25Joint:
#if UNITY_5_5_OR_NEWER
                        lineRenderer.positionCount = 38;
#else
                        lineRenderer.SetVertexCount(38);
#endif
                        currentOrder = Skeleton25JointOrder;
                        break;
                }
            }

            Vector3 pos;
            int index = 0;

            // don't care about below variables, but they're needed
            Quaternion q;
            TrackingState ts;

            foreach (NUIJointType njt in currentOrder)
            {
                pos = Vector3.zero;

                skeletonFrameData.GetJointData(njt, out pos, out q, out ts);

                lineRenderer.SetPosition(index, pos);

                index++;
            }
        }
    }
}
