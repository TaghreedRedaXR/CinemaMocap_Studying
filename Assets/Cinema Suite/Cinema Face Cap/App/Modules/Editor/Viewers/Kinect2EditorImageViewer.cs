
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.UI;
using Microsoft.Kinect.Face;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Windows.Kinect;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.Viewers
{
    [SensorEditorViewer("Image Viewer", 0, SupportedDevice.Kinect2)]
    public class Kinect2EditorImageViewer : Kinect2EditorViewer
    {
        // GUI
        private Texture boundingBoxImage = null;

        // Options
        private bool showBoundingBox = true;

        const int ColorWidth = 1920;
        const int ColorHeight = 1080;

        private Texture2D _ColorTexture;
        private byte[] _ColorData;

        Rect boundingBox = new Rect();
        //List<Vector3> points = new List<Vector3>();

        private MultiSourceFrameReader frameReader;
        //private CoordinateMapper coordinateMapper;

        /// <summary>
        /// Initialize the Kinect1 Image Viewer
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            aspectRatio = 16f / 9f;
            boundingBoxImage = EditorGUIUtility.isProSkin ?
                EditorGUIUtility.Load("Cinema Suite/Cinema Face Cap/CinemaFaceCap_ShowBoundingBox.png") as Texture2D :
                EditorGUIUtility.Load("Cinema Suite/Cinema Face Cap/CinemaFaceCap_ShowBoundingBox_Personal.png") as Texture2D;
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

            showBoundingBox = GUI.Toggle(button2, showBoundingBox, new GUIContent("", boundingBoxImage, "Show Bounding Box"), EditorStyles.toolbarButton);
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

            if (showBoundingBox)
            {
                drawBoundingBox(areaContent);
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

                //coordinateMapper = kinect2Sensor.CoordinateMapper;
            }
        }

        public override void Update(FaceModel faceModel, FaceAlignment faceAlignment)
        {
            base.Update(faceModel, faceAlignment);
            if (faceAlignment != null)
            {
                boundingBox = new Rect(faceAlignment.FaceBoundingBox.Left, faceAlignment.FaceBoundingBox.Top, faceAlignment.FaceBoundingBox.Right, faceAlignment.FaceBoundingBox.Bottom);
            }
        }

        public override void Update()
        {
            
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

        private void drawBoundingBox(Rect areaContent)
        {
            GUILayout.BeginArea(areaContent);
            UnityEngine.Color origHandles = Handles.color;
            Handles.color = UnityEngine.Color.green;

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