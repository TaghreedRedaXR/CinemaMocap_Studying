
using CinemaSuite.CinemaFaceCap.App.Behaviours;
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Capture;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.UI;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using Microsoft.Kinect.Face;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Windows.Kinect;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline.Input
{
    [InputProfileAttribute("Kinect 2 HD", Workflow.Record, 1)]
    public class Kinect2InputProfile : InputProfile
    {
        // The Kinect 2 Sensor
        private KinectSensor kinectSensor;

        private BodyFrameSource bodySource = null;
        private BodyFrameReader bodyReader = null;

        private HighDefinitionFaceFrameSource highDefinitionFaceFrameSource = null;
        private HighDefinitionFaceFrameReader highDefinitionFaceFrameReader = null;

        private FaceAlignment currentFaceAlignment = null;
        private FaceModel currentFaceModel = null;

        private Body currentTrackedBody = null;
        private ulong currentTrackingId = 0;

        private FaceModel CurrentFaceModel
        {
            get
            {
                return currentFaceModel;
            }

            set
            {
                if (currentFaceModel != null)
                {
                    currentFaceModel.Dispose();
                    currentFaceModel = null;
                }

                currentFaceModel = value;
            }
        }


        private bool isPointCloudGenerated = false;
        private GameObject pointCloud;


        // Viewers
        List<TypeLabelContextData> availableViewerTypes = new List<TypeLabelContextData>();
        List<Kinect2EditorViewer> viewers = new List<Kinect2EditorViewer>();

        /// <summary>
        /// Create a new instance of the Kinect 2 Mocap Profile
        /// </summary>
        public Kinect2InputProfile() : base()
        {
            // Get a list of available viewers.
            availableViewerTypes.Clear();
            List<Type> types = SensorEditorViewer.GetSensorEditorViewers(SupportedDevice.Kinect2);
            foreach (Type t in types)
            {
                foreach (SensorEditorViewerAttribute attribute in t.GetCustomAttributes(typeof(SensorEditorViewerAttribute), true))
                {
                    availableViewerTypes.Add(new TypeLabelContextData(t, attribute.Name, attribute.Ordinal));
                }
            }

            availableViewerTypes.Sort(delegate (TypeLabelContextData x, TypeLabelContextData y)
            {
                return x.Ordinal - y.Ordinal;
            });

            AspectRatio = 1f;
        }

        /// <summary>
        /// Destroy the Mocap profile and perform and cleanup.
        /// </summary>
        public override void Destroy()
        {
            GameObject.DestroyImmediate(pointCloud);
            TurnOffDevice();
        }


        public override void Update()
        {
            foreach (var viewer in viewers)
            {
                viewer.Update(currentFaceModel, currentFaceAlignment);
            }
        }

        public override void DrawInputSettings()
        {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Generate Point Cloud"))
            {
                if (pointCloud == null)
                {
                    pointCloud = new GameObject("Point Cloud");
                    pointCloud.AddComponent<PointCloudPreviewer>();
                    pointCloud.transform.Rotate(Vector3.up, 180f);
                    isPointCloudGenerated = true;
                }
            }

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (pointCloud != null)
            {
                UnityEngine.Color temp = GUI.color;
                if (pointCloud.GetComponent<PointCloudPreviewer>().useVerticesSubset == true)
                    GUI.color = UnityEngine.Color.green;
                if (GUILayout.Button("Toggle Subset"))
                    pointCloud.GetComponent<PointCloudPreviewer>().useVerticesSubset = !pointCloud.GetComponent<PointCloudPreviewer>().useVerticesSubset;
                GUI.color = temp;
            }
            else
            {
                if (GUILayout.Button("Toggle Subset")) { } // dummy button
            }
            EditorGUILayout.EndHorizontal();
        }

        public override void DrawDisplayArea(CinemaFaceCapLayout layout)
        {
            // Check that the viewers match the layout.
            syncViewers(layout);

            // Get each viewer's individual area.
            List<Rect> areas = layout.GetViewerRects();

            // Draw each viewer.
            for (int i = 0; i < layout.ViewerCount; i++)
            {
                viewers[i].Area = areas[i];
                viewers[i].DrawBackground();

                // Display the Viewer Type dropdown.
                GUIContent[] content = new GUIContent[availableViewerTypes.Count];
                int currentSelection = 0;
                for (int j = 0; j < availableViewerTypes.Count; j++)
                {
                    content[j] = new GUIContent(availableViewerTypes[j].Label);
                    if (viewers[i].GetType() == availableViewerTypes[j].Type)
                    {
                        currentSelection = j;
                    }
                }
                int tempSelection = EditorGUI.Popup(viewers[i].SelectorArea, currentSelection, content, EditorStyles.toolbarDropDown);
                if (tempSelection != currentSelection)
                {
                    viewers[i] = Activator.CreateInstance(availableViewerTypes[tempSelection].Type) as Kinect2EditorViewer;
                    viewers[i].Initialize(kinectSensor);
                }

                // Draw the rest of the toolbar.
                viewers[i].UpdateToolbar();

                // Draw the contents
                if (IsDeviceOn)
                {
                    viewers[i].DrawContent();
                }
                else
                {
                    viewers[i].DrawPlaceHolder();
                }
            }

        }

        private void syncViewers(CinemaFaceCapLayout layout)
        {
            if (viewers.Count == layout.ViewerCount) { return; }

            // Cache possible types of viewers and existing types of viewers.
            List<Type> viewerTypes = new List<Type>();
            List<Type> existingViewerTypes = new List<Type>();

            foreach (TypeLabelContextData viewerType in availableViewerTypes)
            {
                viewerTypes.Add(viewerType.Type);
            }
            foreach (Kinect2EditorViewer viewer in viewers)
            {
                existingViewerTypes.Add(viewer.GetType());
            }

            // Add or remove viewers as necessary.
            while (viewers.Count != layout.ViewerCount)
            {
                if (viewers.Count < layout.ViewerCount)
                {
                    // Add a new instance of a viewer. Try to add an appropriate one.
                    Type appropriateType = viewerTypes[0];
                    foreach (Type t in viewerTypes)
                    {
                        if (!existingViewerTypes.Contains(t))
                        {
                            appropriateType = t;
                            break;
                        }
                    }

                    Kinect2EditorViewer newViewer = Activator.CreateInstance(appropriateType) as Kinect2EditorViewer;
                    newViewer.Initialize(kinectSensor);
                    viewers.Add(newViewer);

                    existingViewerTypes.Add(appropriateType);
                }
                else if (viewers.Count > layout.ViewerCount)
                {
                    int index = viewers.Count - 1;
                    viewers.RemoveAt(index);
                }
            }
        }

        public override bool TurnOnDevice()
        {
            kinectSensor = KinectSensor.GetDefault();

            if (kinectSensor != null)
            {
                foreach (var viewer in viewers)
                {
                    viewer.SetupReader(kinectSensor);
                }

                bodySource = kinectSensor.BodyFrameSource;
                bodyReader = bodySource.OpenReader();
                bodyReader.FrameArrived += BodyReader_FrameArrived;

                highDefinitionFaceFrameSource = HighDefinitionFaceFrameSource.Create(kinectSensor);
                highDefinitionFaceFrameSource.TrackingIdLost += HighDefinitionFaceFrameSource_TrackingIdLost;

                highDefinitionFaceFrameReader = highDefinitionFaceFrameSource.OpenReader();
                highDefinitionFaceFrameReader.FrameArrived += HighDefinitionFaceFrameReader_FrameArrived;

                CurrentFaceModel = FaceModel.Create();
                currentFaceAlignment = FaceAlignment.Create();

                if (!kinectSensor.IsOpen)
                {
                    kinectSensor.Open();
                }

                return kinectSensor.IsAvailable && kinectSensor.IsOpen;
            }

            return false;
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            var frameReference = e.FrameReference;
            using (var frame = frameReference.AcquireFrame())
            {
                if (frame == null)
                {
                    return;
                }

                if (currentTrackedBody != null)
                {
                    currentTrackedBody = FindBodyWithTrackingId(frame, currentTrackingId);

                    if (currentTrackedBody != null)
                    {
                        return;
                    }
                }

                Body selectedBody = FindClosestBody(frame);

                if (selectedBody == null)
                {
                    return;
                }

                currentTrackedBody = selectedBody;
                currentTrackingId = selectedBody.TrackingId;

                highDefinitionFaceFrameSource.TrackingId = currentTrackingId;
            }
        }

        private void HighDefinitionFaceFrameReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null || !frame.IsFaceTracked)
                {
                    return;
                }

                frame.GetAndRefreshFaceAlignmentResult(currentFaceAlignment);

                var data = new FaceCapFrameData();
                data.FaceBoundingBox = new Rect(currentFaceAlignment.FaceBoundingBox.Left, currentFaceAlignment.FaceBoundingBox.Top, currentFaceAlignment.FaceBoundingBox.Right, currentFaceAlignment.FaceBoundingBox.Bottom);
                data.FaceOrientation = new Quaternion(currentFaceAlignment.FaceOrientation.X, currentFaceAlignment.FaceOrientation.Y, currentFaceAlignment.FaceOrientation.Z, currentFaceAlignment.FaceOrientation.W);

                foreach(var au in currentFaceAlignment.AnimationUnits)
                {
                    data.AnimationUnits.Add(au.Value);
                }

                if (isPointCloudGenerated)
                {
                    if (pointCloud != null)
                    {
                        pointCloud.GetComponent<PointCloudPreviewer>().UpdatePoints(currentFaceModel, currentFaceAlignment);
                    }
                }

                FrameDataEventArgs args = new FrameDataEventArgs(data);
                OnFrameCaptured(args);
            }
        }

        private void HighDefinitionFaceFrameSource_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            var lostTrackingID = e.TrackingId;

            if (currentTrackingId == lostTrackingID)
            {
                currentTrackingId = 0;
                currentTrackedBody = null;

                highDefinitionFaceFrameSource.TrackingId = 0;
            }
        }

        private Body FindClosestBody(BodyFrame frame)
        {
            Body result = null;
            double closestBodyDistance = double.MaxValue;

            Body[] bodies = new Body[frame.BodyCount];
            frame.GetAndRefreshBodyData(bodies);

            foreach (var body in bodies)
            {
                if (body.IsTracked)
                {
                    var currentLocation = body.Joints[JointType.SpineBase].Position;

                    var currentDistance = VectorLength(currentLocation);

                    if (result == null || currentDistance < closestBodyDistance)
                    {
                        result = body;
                        closestBodyDistance = currentDistance;
                    }
                }
            }

            return result;
        }

        private Body FindBodyWithTrackingId(BodyFrame frame, ulong currentTrackingId)
        {
            Body result = null;

            Body[] bodies = new Body[frame.BodyCount];
            frame.GetAndRefreshBodyData(bodies);

            foreach (var body in bodies)
            {
                if (body.IsTracked)
                {
                    if (body.TrackingId == currentTrackingId)
                    {
                        result = body;
                        break;
                    }
                }
            }

            return result;
        }

        public override void TurnOffDevice()
        {
            if (kinectSensor != null)
            {
                if (kinectSensor.IsOpen)
                {
                    kinectSensor.Close();
                }

                kinectSensor = null;
            }
        }

        public override bool IsDeviceOn
        {
            get
            {
                return kinectSensor != null && kinectSensor.IsAvailable && kinectSensor.IsOpen;
            }
        }

        public override bool ShowInputSettings
        {
            get
            {
                return false;
            }
        }

        public override FaceCapSessionMetaData GetSessionMetaData()
        {
            var metaData = new FaceCapSessionMetaData();
            metaData.captureDevice = SupportedDevice.Kinect2;

            return metaData;
        }

        public override InputFace InputFace
        {
            get
            {
                return InputFace.SeventeenAnimationUnits;
            }
        }

        private static double VectorLength(CameraSpacePoint point)
        {
            var result = Mathf.Pow(point.X, 2) + Mathf.Pow(point.Y, 2) + Mathf.Pow(point.Z, 2);

            result = Mathf.Sqrt(result);

            return result;
        }
    }
}