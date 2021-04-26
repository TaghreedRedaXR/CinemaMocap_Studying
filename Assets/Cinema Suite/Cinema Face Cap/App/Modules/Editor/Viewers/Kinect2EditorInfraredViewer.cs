
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.UI;
using Microsoft.Kinect.Face;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Windows.Kinect;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.Viewers
{
    [SensorEditorViewer("Infrared Viewer", 1, SupportedDevice.Kinect2)]
    public class Kinect2EditorInfraredViewer : Kinect2EditorViewer
    {
        // GUI
        private Texture boundingBoxImage = null;

        // Options
        private bool showBoundingBox = true;

        const int ColorWidth = 1920;
        const int ColorHeight = 1080;

        private InfraredFrameReader infraredReader;
        private Texture2D _InfraredTexture;
        private ushort[] _InfraredData;
        private byte[] _InfraredRawData;

        //private CoordinateMapper coordinateMapper;

        Rect boundingBox = new Rect();

        /// <summary>
        /// Initialize the Kinect1 Image Viewer
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            aspectRatio = 16f / 9f;

            boundingBoxImage = EditorGUIUtility.Load("Cinema Suite/Cinema Face Cap/CinemaFaceCap_ShowBoundingBox.png") as Texture2D;
            if (boundingBoxImage == null)
            {
                UnityEngine.Debug.LogWarning("CinemaFaceCap_ShowBoundingBox.png is missing from Resources folder.");
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

            //showBoundingBox = GUI.Toggle(button2, showBoundingBox, new GUIContent("", boundingBoxImage, "Show Bounding Box"), EditorStyles.toolbarButton);
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

            GUI.DrawTexture(new Rect(areaContent.x, areaContent.y + areaContent.height, areaContent.width, -areaContent.height), _InfraredTexture);

            if (showBoundingBox)
            {
                //drawBoundingBox(areaContent);
            }
        }

        public override void SetupReader(Windows.Kinect.KinectSensor kinect2Sensor)
        {
            if (kinect2Sensor != null)
            {
                infraredReader = kinect2Sensor.InfraredFrameSource.OpenReader();
                var infraredFrameDesc = kinect2Sensor.InfraredFrameSource.FrameDescription;
                _InfraredData = new ushort[infraredFrameDesc.LengthInPixels];
                _InfraredRawData = new byte[infraredFrameDesc.LengthInPixels * 4];
                _InfraredTexture = new Texture2D(infraredFrameDesc.Width, infraredFrameDesc.Height, TextureFormat.BGRA32, false);

                //coordinateMapper = kinect2Sensor.CoordinateMapper;
            }
        }

        public override void Update(FaceModel faceModel, FaceAlignment faceAlignment)
        {
            base.Update(faceModel, faceAlignment);

            if (faceAlignment != null)
                boundingBox = new Rect(faceAlignment.FaceBoundingBox.Left, faceAlignment.FaceBoundingBox.Top, faceAlignment.FaceBoundingBox.Right, faceAlignment.FaceBoundingBox.Bottom);
        }

        public override void Update()
        {

            if (infraredReader != null)
            {
                var infraredFrame = infraredReader.AcquireLatestFrame();
                if (infraredFrame != null)
                {
                    infraredFrame.CopyFrameDataToArray(_InfraredData);

                    int index = 0;
                    foreach (var ir in _InfraredData)
                    {
                        byte intensity = (byte)(ir >> 8);
                        _InfraredRawData[index++] = intensity;
                        _InfraredRawData[index++] = intensity;
                        _InfraredRawData[index++] = intensity;
                        _InfraredRawData[index++] = 255;
                    }

                    _InfraredTexture.LoadRawTextureData(_InfraredRawData);
                    _InfraredTexture.Apply();

                    infraredFrame.Dispose();
                    infraredFrame = null;
                }
            }
        }


        private void drawBoundingBox(Rect areaContent)
        {
            GUILayout.BeginArea(areaContent);
            UnityEngine.Color origHandles = Handles.color;

            GUI.color = UnityEngine.Color.green;

            Handles.DrawLine(new Vector3((boundingBox.x / ColorWidth) * areaContent.width, (boundingBox.y / ColorHeight) * areaContent.height, 0),
                new Vector3((boundingBox.width / ColorWidth) * areaContent.width, (boundingBox.y / ColorHeight) * areaContent.height, 0));

            Handles.DrawLine(new Vector3((boundingBox.width / ColorWidth) * areaContent.width, (boundingBox.y / ColorHeight) * areaContent.height, 0),
                new Vector3((boundingBox.width / ColorWidth) * areaContent.width, (boundingBox.height / ColorHeight) * areaContent.height, 0));

            Handles.DrawLine(new Vector3((boundingBox.width / ColorWidth) * areaContent.width, (boundingBox.height / ColorHeight) * areaContent.height, 0),
                new Vector3((boundingBox.x / ColorWidth) * areaContent.width, (boundingBox.height / ColorHeight) * areaContent.height, 0));

            Handles.DrawLine(new Vector3((boundingBox.x / ColorWidth) * areaContent.width, (boundingBox.height / ColorHeight) * areaContent.height, 0),
                new Vector3((boundingBox.x / ColorWidth) * areaContent.width, (boundingBox.y / ColorHeight) * areaContent.height, 0));

            Handles.color = origHandles;
            GUILayout.EndArea();
        }
    }
}