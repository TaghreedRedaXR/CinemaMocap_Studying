
using Microsoft.Kinect.Face;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;

namespace CinemaSuite.CinemaFaceCap.App.Behaviours
{
    public class PointCloudPreviewer : MonoBehaviour
    {
        List<GameObject> points = new List<GameObject>();
        private bool _useVerticesSubset = false;

        public void UpdatePoints(FaceModel currentFaceModel, FaceAlignment currentFaceAlignment)
        {
            var vertices = currentFaceModel.CalculateVerticesForAlignment(currentFaceAlignment);
            var subset = new List<CameraSpacePoint>();

            if (vertices.Count > 0)
            {
                if (useVerticesSubset)
                {
                    subset = new List<CameraSpacePoint>()
                    {
                        {vertices[(int)HighDetailFacePoints.LefteyeInnercorner]},
                        {vertices[(int)HighDetailFacePoints.LefteyeOutercorner]},
                        {vertices[(int)HighDetailFacePoints.LefteyeMidtop]},
                        {vertices[(int)HighDetailFacePoints.LefteyeMidbottom]},
                        {vertices[(int)HighDetailFacePoints.RighteyeInnercorner]},
                        {vertices[(int)HighDetailFacePoints.RighteyeOutercorner]},
                        {vertices[(int)HighDetailFacePoints.RighteyeMidtop]},
                        {vertices[(int)HighDetailFacePoints.RighteyeMidbottom]},
                        {vertices[(int)HighDetailFacePoints.LefteyebrowInner]},
                        {vertices[(int)HighDetailFacePoints.LefteyebrowOuter]},
                        {vertices[(int)HighDetailFacePoints.LefteyebrowCenter]},
                        {vertices[(int)HighDetailFacePoints.RighteyebrowInner]},
                        {vertices[(int)HighDetailFacePoints.RighteyebrowOuter]},
                        {vertices[(int)HighDetailFacePoints.RighteyebrowCenter]},
                        {vertices[(int)HighDetailFacePoints.MouthLeftcorner]},
                        {vertices[(int)HighDetailFacePoints.MouthRightcorner]},
                        {vertices[(int)HighDetailFacePoints.MouthUpperlipMidtop]},
                        {vertices[(int)HighDetailFacePoints.MouthUpperlipMidbottom]},
                        {vertices[(int)HighDetailFacePoints.MouthLowerlipMidtop]},
                        {vertices[(int)HighDetailFacePoints.MouthLowerlipMidbottom]},
                        {vertices[(int)HighDetailFacePoints.NoseTip]},
                        {vertices[(int)HighDetailFacePoints.NoseBottom]},
                        {vertices[(int)HighDetailFacePoints.NoseBottomleft]},
                        {vertices[(int)HighDetailFacePoints.NoseBottomright]},
                        {vertices[(int)HighDetailFacePoints.NoseTop]},
                        {vertices[(int)HighDetailFacePoints.NoseTopleft]},
                        {vertices[(int)HighDetailFacePoints.NoseTopright]},
                        {vertices[(int)HighDetailFacePoints.ForeheadCenter]},
                        {vertices[(int)HighDetailFacePoints.LeftcheekCenter]},
                        {vertices[(int)HighDetailFacePoints.RightcheekCenter]},
                        {vertices[(int)HighDetailFacePoints.Leftcheekbone]},
                        {vertices[(int)HighDetailFacePoints.Rightcheekbone]},
                        {vertices[(int)HighDetailFacePoints.ChinCenter]},
                        {vertices[(int)HighDetailFacePoints.LowerjawLeftend]},
                        {vertices[(int)HighDetailFacePoints.LowerjawRightend]},
                    };
                    vertices = subset;
                }
                
                if (points.Count == 0 )
                {
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        go.transform.parent = this.gameObject.transform;
                        go.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);

                        points.Add(go);
                    }
                }

                for (int i = 0; i < vertices.Count; i++)
                {
                    var vertex = vertices[i];
                    points[i].transform.localPosition = new Vector3(vertex.X, vertex.Y, vertex.Z);
                }
            }
        }

        public void ClearPoints()
        {
            foreach (var point in points)
               DestroyImmediate(point);
            points.RemoveAll(item => item == null);
            
        }

        public bool useVerticesSubset
        {
            get { return _useVerticesSubset; }
            set
            {
                _useVerticesSubset = value;
                ClearPoints();
            }
        }
    }
}