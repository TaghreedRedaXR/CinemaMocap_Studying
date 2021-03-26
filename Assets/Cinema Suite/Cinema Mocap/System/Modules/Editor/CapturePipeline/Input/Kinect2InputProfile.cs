using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Capture;
using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Windows.Kinect;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline.Input
{
    [InputProfileAttribute("Kinect 2", MocapWorkflow.Record, 1)]
    public class Kinect2InputProfile : InputProfile
    {
        // The Kinect 2 Sensor
        private KinectSensor kinectSensor;

        // The Body data
        private BodyFrameReader bodyReader;
        private Body[] bodyData;
        private Body currentBody;


        private InputSkeletonType inputSkeletonType = InputSkeletonType.Kinect2_25Joint;

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
            TurnOffDevice();
        }

        public override void Update()
        {
            if (bodyReader != null)
            {
                getMainUser();
            }

            foreach (var viewer in viewers)
            {
                viewer.Update(bodyData, 0);
            }

            // Get the pose.
            if (currentBody != null && currentBody.IsTracked)
            {
                // Encapsulate the important frame data
                SkeletonFrameData frameData = new SkeletonFrameData();
                frameData.TrackingId = currentBody.TrackingId;
                frameData.IsTracked = currentBody.IsTracked;

                // Build the skeleton
                for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++)
                {
                    NUIJointType jointType = NUICaptureHelper.JointTypeToNUIJointTypeMapping(jt);

                    Vector3 position = new Vector3(currentBody.Joints[jt].Position.X, currentBody.Joints[jt].Position.Y, currentBody.Joints[jt].Position.Z);
                    position.z *= -1; // Should probably be done with meta data/mapper to avoid disturbing raw data.

                    Quaternion orientation = new Quaternion(currentBody.JointOrientations[jt].Orientation.X, currentBody.JointOrientations[jt].Orientation.Y, currentBody.JointOrientations[jt].Orientation.Z, currentBody.JointOrientations[jt].Orientation.W);

                    frameData.AddJoint(jointType, position, orientation, (CinemaSuite.CinemaMocap.System.Core.TrackingState)currentBody.Joints[jt].TrackingState);
                }

                // Hand info
                frameData.LeftHandConfidence = (CinemaSuite.CinemaMocap.System.Core.TrackingConfidence)currentBody.HandLeftConfidence;
                frameData.LeftHandState = (CinemaSuite.CinemaMocap.System.Core.HandState)currentBody.HandLeftState;

                frameData.RightHandConfidence = (CinemaSuite.CinemaMocap.System.Core.TrackingConfidence)currentBody.HandRightConfidence;
                frameData.RightHandState = (CinemaSuite.CinemaMocap.System.Core.HandState)currentBody.HandRightState;

                // Frame info
                frameData.ClippedEdges = (CinemaSuite.CinemaMocap.System.Core.FrameEdges)currentBody.ClippedEdges;

                FrameDataEventArgs args = new FrameDataEventArgs(frameData);
                OnFrameCaptured(args);
            }
        }

        private void getMainUser()
        {
            var bodyFrame = bodyReader.AcquireLatestFrame();
            if (bodyFrame != null)
            {
                if (bodyData == null)
                {
                    bodyData = new Body[kinectSensor.BodyFrameSource.BodyCount];
                }

                bodyFrame.GetAndRefreshBodyData(bodyData);

                foreach (var body in bodyData)
                {
                    if (body == null)
                    {
                        continue;
                    }

                    if (body.IsTracked)
                    {
                        currentBody = body;
                    }
                }

                bodyFrame.Dispose();
                bodyFrame = null;
            }
        }

        public override void DrawInputSettings() { }

        public override void DrawDisplayArea(CinemaMocapLayout layout)
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

        private void syncViewers(CinemaMocapLayout layout)
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

                bodyReader = kinectSensor.BodyFrameSource.OpenReader();


                if (!kinectSensor.IsOpen)
                {
                    kinectSensor.Open();
                }

                return kinectSensor.IsAvailable && kinectSensor.IsOpen;
            }

            return false;
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

        public override InputSkeletonType InputSkeleton
        {
            get
            {
                return inputSkeletonType;
            }
        }

        public override MocapSessionMetaData GetSessionMetaData()
        {
            MocapSessionMetaData metaData = new MocapSessionMetaData();
            metaData.CaptureDevice = SupportedDevice.Kinect2;
            metaData.InputSkeletonType = inputSkeletonType;
            metaData.IsFrameEdgeDataAvailable = true;
            metaData.IsHandDataAvailable = true;

            return metaData;
        }
    }
}