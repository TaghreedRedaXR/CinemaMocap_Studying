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

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline.Input
{
    [InputProfileAttribute("Kinect 1", MocapWorkflow.Record, 1)]
    public class Kinect1InputProfile : InputProfile
    {
        private ZigInputType inputType = ZigInputType.KinectSDK;
        private ZigInputSettings settings = new ZigInputSettings();
        private MocapSession session;
        private InputSkeletonType inputSkeletonType = InputSkeletonType.Kinect1_20Joint;

        // Device Settings
        private float desiredTilt = 0f;
        private SmoothingOptions smoothingOptions = SmoothingOptions.Light;

        // GUI Content
        private GUIContent SMOOTHING_OPTIONS = new GUIContent("Hardware Smoothing");
        private GUIContent DEVICE_TILT = new GUIContent("Device Tilt");

        // Viewers
        List<TypeLabelContextData> availableViewerTypes = new List<TypeLabelContextData>();
        List<Kinect1EditorViewer> viewers = new List<Kinect1EditorViewer>();

        public Kinect1InputProfile() : base()
        {
            // Get a list of available viewers.
            availableViewerTypes.Clear();

            List<Type> types = SensorEditorViewer.GetSensorEditorViewers(SupportedDevice.Kinect1);
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

            AspectRatio = 4f / 3f;
        }

        /// <summary>
        /// Destroy any assets that were loaded.
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Update()
        {
            if (ZigEditorInput.Instance.ReaderInited)
            {
                // Update Device
                ZigEditorInput.Instance.Update();

                // Get the tracked user
                ZigTrackedUser user = null;
                int userId = 0;
                foreach (KeyValuePair<int, ZigTrackedUser> trackedUser in ZigEditorInput.Instance.TrackedUsers)
                {
                    user = trackedUser.Value;
                    userId = trackedUser.Key;
                }

                // Update viewers
                foreach (Kinect1EditorViewer viewer in viewers)
                {
                    viewer.Update(ZigEditorInput.Instance, userId);
                }

                if (user != null && user.SkeletonTracked)
                {
                    // Encapsulate the important frame data
                    SkeletonFrameData frameData = new SkeletonFrameData();
                    frameData.TrackingId = (ulong)user.Id;
                    frameData.IsTracked = user.SkeletonTracked;

                    foreach (ZigInputJoint inputJoint in user.Skeleton)
                    {
                        NUIJointType jointType = NUICaptureHelper.ZigToNUIJointMapping(inputJoint.Id);

                        // Convert position from mm to meters
                        Vector3 position = inputJoint.Position / 1000f;
                        Quaternion orientation = inputJoint.Rotation;

                        frameData.AddJoint(jointType, position, orientation, inputJoint.Inferred ? TrackingState.Inferred : TrackingState.Tracked);
                    }

                    FrameDataEventArgs args = new FrameDataEventArgs(frameData);
                    OnFrameCaptured(args);
                }
            }
        }

        /// <summary>
        /// Draw the options panel for hardware device settings.
        /// </summary>
        public override void DrawInputSettings()
        {
            SmoothingOptions newSmoothingOptions = (SmoothingOptions)EditorGUILayout.EnumPopup(new GUIContent(SMOOTHING_OPTIONS), smoothingOptions);
            if (newSmoothingOptions != smoothingOptions)
            {
                smoothingOptions = newSmoothingOptions;
                if (smoothingOptions == SmoothingOptions.None)
                {
                    settings.KinectSDKSpecific.SmoothingParameters.SetNoSmoothing();
                }
                else if (smoothingOptions == SmoothingOptions.Light)
                {
                    settings.KinectSDKSpecific.SmoothingParameters.SetLightSmoothing();
                }
                else if (smoothingOptions == SmoothingOptions.Moderate)
                {
                    settings.KinectSDKSpecific.SmoothingParameters.SetModerateSmoothing();
                }
                else if (smoothingOptions == SmoothingOptions.Heavy)
                {
                    settings.KinectSDKSpecific.SmoothingParameters.Smoothing = 0.6f;
                    settings.KinectSDKSpecific.SmoothingParameters.Correction = 0.4f;
                    settings.KinectSDKSpecific.SmoothingParameters.Prediction = 0.6f;
                    settings.KinectSDKSpecific.SmoothingParameters.JitterRadius = 0.15f;
                    settings.KinectSDKSpecific.SmoothingParameters.MaxDeviationRadius = 0.10f;
                }
                else if (smoothingOptions == SmoothingOptions.Custom)
                {
                    settings.KinectSDKSpecific.SmoothingParameters.SetModerateSmoothing();
                }
                if (ZigEditorInput.Instance.ReaderInited)
                {
                    ZigEditorInput.Instance.SetSmoothingParameters((smoothingOptions != SmoothingOptions.None), settings.KinectSDKSpecific.SmoothingParameters);
                }
            }

            if (smoothingOptions == SmoothingOptions.Custom)
            {
                EditorGUI.indentLevel++;
                float tempSmoothing = EditorGUILayout.Slider(new GUIContent("Smoothing"), settings.KinectSDKSpecific.SmoothingParameters.Smoothing, 0f, 1f);
                float tempCorrection = EditorGUILayout.Slider(new GUIContent("Correction"), settings.KinectSDKSpecific.SmoothingParameters.Correction, 0f, 1f);
                float tempPrediction = EditorGUILayout.Slider(new GUIContent("Prediction"), settings.KinectSDKSpecific.SmoothingParameters.Prediction, 0f, 1f);
                float tempJitterRadius = EditorGUILayout.Slider(new GUIContent("JitterRadius"), settings.KinectSDKSpecific.SmoothingParameters.JitterRadius, 0f, 1f);
                float tempMaxDeviationRadius = EditorGUILayout.Slider(new GUIContent("MaxDeviationRadius"), settings.KinectSDKSpecific.SmoothingParameters.MaxDeviationRadius, 0f, 1f);
                EditorGUI.indentLevel--;

                if (settings.KinectSDKSpecific.SmoothingParameters.Smoothing != tempSmoothing ||
                    settings.KinectSDKSpecific.SmoothingParameters.Correction != tempCorrection ||
                        settings.KinectSDKSpecific.SmoothingParameters.Prediction != tempPrediction ||
                            settings.KinectSDKSpecific.SmoothingParameters.JitterRadius != tempJitterRadius ||
                                settings.KinectSDKSpecific.SmoothingParameters.MaxDeviationRadius != tempMaxDeviationRadius)
                {
                    settings.KinectSDKSpecific.SmoothingParameters.Smoothing = tempSmoothing;
                    settings.KinectSDKSpecific.SmoothingParameters.Correction = tempCorrection;
                    settings.KinectSDKSpecific.SmoothingParameters.Prediction = tempPrediction;
                    settings.KinectSDKSpecific.SmoothingParameters.JitterRadius = tempJitterRadius;
                    settings.KinectSDKSpecific.SmoothingParameters.MaxDeviationRadius = tempMaxDeviationRadius;
                    if (ZigEditorInput.Instance.ReaderInited)
                    {
                        ZigEditorInput.Instance.SetSmoothingParameters(true, settings.KinectSDKSpecific.SmoothingParameters);
                    }
                }

            }

            float newDesiredTilt = desiredTilt;

            EditorGUI.BeginDisabledGroup(!IsDeviceOn);
            newDesiredTilt = EditorGUILayout.IntSlider(new GUIContent(DEVICE_TILT), (int)desiredTilt, -27, 27);
            EditorGUI.EndDisabledGroup();

            if (newDesiredTilt != desiredTilt)
            {
                desiredTilt = newDesiredTilt;
                NuiWrapper.NuiCameraElevationSetAngle((long)desiredTilt);
            }
        }

        /// <summary>
        /// Draw the display area.
        /// </summary>
        /// <param name="layout">The layout of the display area.</param>
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
                    viewers[i] = Activator.CreateInstance(availableViewerTypes[tempSelection].Type) as Kinect1EditorViewer;
                    viewers[i].Initialize();
                }

                // Draw the rest of the toolbar.
                viewers[i].UpdateToolbar();

                // Draw the contents
                if (ZigEditorInput.Instance.ReaderInited)
                {
                    viewers[i].DrawContent();
                }
                else
                {
                    viewers[i].DrawPlaceHolder();
                }
            }
        }

        /// <summary>
        /// Sync the collection of Viewers to the requested amount of the layout.
        /// </summary>
        /// <param name="layout">The layout to sync with.</param>
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
            foreach (Kinect1EditorViewer viewer in viewers)
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

                    Kinect1EditorViewer newViewer = Activator.CreateInstance(appropriateType) as Kinect1EditorViewer;
                    newViewer.Initialize();
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

        /// <summary>
        /// Attempt to turn on the Kinect 1 device.
        /// </summary>
        /// <returns>Returns true if Kinect 1 is initialized.</returns>
        public override bool TurnOnDevice()
        {
            settings.UpdateImage = true;

            ZigEditorInput.InputType = inputType;
            ZigEditorInput.Settings = settings;

            ZigEditorInput.Instance.Init();

            long tilt = (long)desiredTilt;
            NuiWrapper.NuiCameraElevationGetAngle(out tilt);
            desiredTilt = (float)tilt;

            return ZigEditorInput.Instance.ReaderInited;
        }

        /// <summary>
        /// Turn off the Kinect 1.
        /// </summary>
        public override void TurnOffDevice()
        {
            ZigEditorInput.Instance.ShutdownReader();
        }

        public override bool IsDeviceOn
        {
            get { return ZigEditorInput.Instance.ReaderInited; }
        }

        public override bool ShowInputSettings
        {
            get
            {
                return true;
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
            metaData.CaptureDevice = SupportedDevice.Kinect1;
            metaData.InputSkeletonType = this.InputSkeleton;
            metaData.IsFrameEdgeDataAvailable = false;
            metaData.IsHandDataAvailable = false;

            return metaData;
        }
    }
}