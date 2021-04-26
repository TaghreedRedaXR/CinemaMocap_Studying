
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Core.Capture
{
    [Serializable]
    public class FaceCapFrameData
    {
        [SerializeField]
        private Quaternion faceOrientation;

        [SerializeField]
        private Rect faceBoundingBox;

        [SerializeField]
        private Vector3 headPivotPoint;

        [SerializeField]
        private List<float> animationUnits = new List<float>();

        public Quaternion FaceOrientation
        {
            get
            {
                return faceOrientation;
            }

            set
            {
                faceOrientation = value;
            }
        }

        public Rect FaceBoundingBox
        {
            get
            {
                return faceBoundingBox;
            }

            set
            {
                faceBoundingBox = value;
            }
        }

        public Vector3 HeadPivotPoint
        {
            get
            {
                return headPivotPoint;
            }

            set
            {
                headPivotPoint = value;
            }
        }

        public List<float> AnimationUnits
        {
            get
            {
                return animationUnits;
            }

            set
            {
                animationUnits = value;
            }
        }
    }
}