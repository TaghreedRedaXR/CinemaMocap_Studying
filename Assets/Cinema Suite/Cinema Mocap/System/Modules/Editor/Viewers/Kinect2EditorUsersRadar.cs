using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Windows.Kinect;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.Viewers
{
    [SensorEditorViewer("Radar", 3, SupportedDevice.Kinect2)]
    public class Kinect2EditorUsersRadar : Kinect2EditorViewer
    {
        private List<Vector2> userPositions = new List<Vector2>();
        private Texture2D radarTracker;

        public override void Initialize()
        {
            base.Initialize();

            string res_dir = "Cinema Suite/Cinema Mocap/";

            base.PlaceHolderImage = EditorGUIUtility.Load(res_dir + "Radar1.png") as Texture2D;
            if (PlaceHolderImage == null)
            {
                UnityEngine.Debug.LogWarning("Radar1.png is missing from Resources folder.");
            }

            radarTracker = EditorGUIUtility.Load(res_dir + "Radar Tracker.png") as Texture2D;
            if (radarTracker == null)
            {
                UnityEngine.Debug.LogWarning("Radar Tracker.png is missing from Resources folder.");
            }
        }

        public override void DrawContent()
        {
 	        DrawPlaceHolder();

            Color orig = GUI.color;
            GUI.color = Color.cyan;
            foreach(Vector2 position in userPositions)
            {
                GUI.DrawTexture(new Rect(position.x * ContentBackgroundArea.width + (area.x - 25), position.y * ContentBackgroundArea.height + (area.y - 25), 50, 50), radarTracker);
            }
            GUI.color = orig;
        }

        public override void Update()
        {
            userPositions.Clear();
        }

        public override void SetupReader(KinectSensor kinect2Sensor)
        {
        }

        public override void Update(Windows.Kinect.Body[] bodies, int mainUserId)
        {
 	        base.Update(bodies, mainUserId);

            if (bodies != null)
            {
                Body currentBody = null;
                 
                foreach (var body in bodies)
                {
                    if (body.IsTracked)
                    {
                        currentBody = body;
                    }
                    if (currentBody != null)
                    {
                        CameraSpacePoint csp = currentBody.Joints[JointType.SpineBase].Position;
                        Vector3 position = new Vector3(csp.X, csp.Y, csp.Z);
                        if (position != Vector3.zero)
                        {
                            Vector2 radarPosition = new Vector2(position.x / 4, position.z / 4);

                            // X axis: 0 in real world is actually 0.5 in radar units (middle of field of view)
                            radarPosition.x += 0.5f;

                            // clamp
                            radarPosition.x = Mathf.Clamp(radarPosition.x, 0.0f, 1.0f);
                            radarPosition.y = Mathf.Clamp(radarPosition.y, 0.0f, 1.0f);

                            userPositions.Add(radarPosition);
                                 
                        }
                    }
                }
            }
        }
    }
}