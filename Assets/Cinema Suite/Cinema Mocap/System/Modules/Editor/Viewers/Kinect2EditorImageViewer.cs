using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Windows.Kinect;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.Viewers
{
    [SensorEditorViewer("Image Viewer", 0, SupportedDevice.Kinect2)]
    public class Kinect2EditorImageViewer : Kinect2EditorViewer
    {
        // GUI
        private Texture nodesImage = null;
        private Texture bonesImage = null;
        private Texture jointTrackerImage = null;

        // Options
        private bool showNodes = true;
        private bool showBones = true;

        const int ColorWidth = 1920;
        const int ColorHeight = 1080;

        private Texture2D _ColorTexture;
        private byte[] _ColorData;

        private MultiSourceFrameReader frameReader;
        private CoordinateMapper coordinateMapper;

        private List<Dictionary<JointType, Windows.Kinect.Joint>> trackedBodyJoints = new List<Dictionary<JointType, Windows.Kinect.Joint>>();

        /// <summary>
        /// Initialize the Kinect1 Image Viewer
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            aspectRatio = 16f / 9f;
            string res_dir = "Cinema Suite/Cinema Mocap/";

            string showNodesName = EditorGUIUtility.isProSkin ? "CinemaMocap_ShowNodes" : "CinemaMocap_ShowNodes_Personal";
            nodesImage = EditorGUIUtility.Load(res_dir + showNodesName + ".png") as Texture2D;
            if (nodesImage == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("{0}.png is missing from Resources folder.", showNodesName));
            }

            string showSkeletonName = EditorGUIUtility.isProSkin ? "CinemaMocap_ShowSkeleton" : "CinemaMocap_ShowSkeleton_Personal";
            bonesImage = EditorGUIUtility.Load(res_dir + showSkeletonName + ".png") as Texture2D;
            if (bonesImage == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("{0}.png is missing from Resources folder.", showSkeletonName));
            }

            jointTrackerImage = EditorGUIUtility.Load(res_dir + "Joint Tracker.png") as Texture2D;
            if (jointTrackerImage == null)
            {
                UnityEngine.Debug.LogWarning("Joint Tracker.png is missing from Resources folder.");
            }
        }

        public override void UpdateToolbar()
        {
            Rect button1 = toolbarLeftover;
            button1.width = 24;
            button1.x = toolbarLeftover.x + toolbarLeftover.width - 58;

            Rect button2 = toolbarLeftover;
            button2.width = 24;
            button2.x = toolbarLeftover.x + toolbarLeftover.width - 29;

            Rect resolutionArea = button1;
            resolutionArea.x -= 120;
            resolutionArea.width = 120;

            showNodes = GUI.Toggle(button1, showNodes, new GUIContent("", nodesImage, "Show Nodes"), EditorStyles.toolbarButton);
            showBones = GUI.Toggle(button2, showBones, new GUIContent("", bonesImage, "Show Bones"), EditorStyles.toolbarButton);
        }

        public override void DrawContent()
        {
            float textureHeight = ContentBackgroundArea.height;
            float textureWidth = textureHeight * aspectRatio;

            if ((ContentBackgroundArea.width / ContentBackgroundArea.height) < aspectRatio)
            {
                textureWidth = ContentBackgroundArea.width;
                textureHeight = textureWidth * (1f / aspectRatio);
            }
            Rect areaContent = new Rect((ContentBackgroundArea.x + (ContentBackgroundArea.width - textureWidth) / 2) + 2, (ContentBackgroundArea.y + (ContentBackgroundArea.height - textureHeight) / 2) + 2, textureWidth - 4, textureHeight - 4);

            GUI.DrawTexture(new Rect(areaContent.x, areaContent.y + areaContent.height, areaContent.width, -areaContent.height), _ColorTexture);

            if (showBones)
            {
                DrawBones(areaContent);
            }
            if (showNodes)
            {
                DrawJoints(areaContent);
            }
        }

        public override void SetupReader(Windows.Kinect.KinectSensor kinect2Sensor)
        {
            if (kinect2Sensor != null)
            {
                frameReader = kinect2Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color);
                var colorFrameDesc = kinect2Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);

                _ColorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
                _ColorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];

                coordinateMapper = kinect2Sensor.CoordinateMapper;
            }
        }

        public override void Update(Body[] bodies, int mainUserId)
        {
            base.Update(bodies, mainUserId);

            if (bodies != null)
            {
                Body currentBody = null;
                if (showNodes || showBones)
                {
                    foreach (var body in bodies)
                    {
                        if (body.IsTracked)
                        {
                            currentBody = body;
                        }


                        if (currentBody != null && currentBody.IsTracked)
                        {
                            trackedBodyJoints.Add(currentBody.Joints);
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            trackedBodyJoints.Clear();

            if (frameReader != null)
            {
                var frame = frameReader.AcquireLatestFrame();
                if (frame != null)
                {
                    var colorFrame = frame.ColorFrameReference.AcquireFrame();
                    if (colorFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(_ColorData, ColorImageFormat.Rgba);
                        _ColorTexture.LoadRawTextureData(_ColorData);

                        _ColorTexture.Apply();

                        colorFrame.Dispose();
                        colorFrame = null;
                    }
                }
                frame = null;
            }
        }

        private void DrawJoints(Rect areaContent)
        {
            GUILayout.BeginArea(areaContent);
            Color orig2 = GUI.color;

            foreach (Dictionary<JointType, Windows.Kinect.Joint> jointDictionary in trackedBodyJoints) // each dictionary is a different body
            {
                foreach (Windows.Kinect.Joint joint in jointDictionary.Values)
                {
                    ColorSpacePoint screenspace = coordinateMapper.MapCameraPointToColorSpace(joint.Position);
                    if (joint.TrackingState == Windows.Kinect.TrackingState.Tracked)
                    {
                        GUI.color = Color.green;
                    }
                    else if (joint.TrackingState == Windows.Kinect.TrackingState.Inferred)
                    {
                        GUI.color = Color.red;
                    }

                    var jointRect = new Rect(- 8 + (screenspace.X / ColorWidth) * areaContent.width, - 8 + (screenspace.Y / ColorHeight) * areaContent.height, 16, 16);
                    GUI.DrawTexture(jointRect, jointTrackerImage);
                }
            }

            GUI.color = orig2;
            GUILayout.EndArea();
        }


        private void DrawBones(Rect areaContent)
        {
            GUILayout.BeginArea(areaContent);
            Color origHandles = Handles.color;

            foreach (Dictionary<JointType, Windows.Kinect.Joint> jointDictionary in trackedBodyJoints)
            {
                foreach (Windows.Kinect.Joint joint in jointDictionary.Values)
                {
                    // find parent joint
                    JointType parentJointType = CinemaMocapHelper.ParentBoneJoint(joint.JointType);
                    Windows.Kinect.Joint parentJoint;

                    jointDictionary.TryGetValue(parentJointType, out parentJoint);

                    // get position of each joint
                    ColorSpacePoint childPos = coordinateMapper.MapCameraPointToColorSpace(joint.Position);
                    ColorSpacePoint parentPos = coordinateMapper.MapCameraPointToColorSpace(parentJoint.Position);

                    // set color depending on child joint tracking state
                    if (joint.TrackingState == Windows.Kinect.TrackingState.Tracked)
                    {
                        Handles.color = Color.green;
                    }
                    else if (joint.TrackingState == Windows.Kinect.TrackingState.Inferred)
                    {
                        Handles.color = Color.red;
                    }

                    // draw bone/line between joints using CinemaMocapHelper
                    Handles.DrawLine(new Vector3((childPos.X / ColorWidth) * areaContent.width, (childPos.Y / ColorHeight) * areaContent.height, 0), new Vector3((parentPos.X / ColorWidth) * areaContent.width,(parentPos.Y / ColorHeight) * areaContent.height, 0));
                }
            }

            Handles.color = origHandles;
            GUILayout.EndArea();
        }
    }
}