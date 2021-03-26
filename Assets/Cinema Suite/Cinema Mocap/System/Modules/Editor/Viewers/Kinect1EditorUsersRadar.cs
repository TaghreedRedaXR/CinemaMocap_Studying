using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.Viewers
{
    [SensorEditorViewer("Radar", 3, SupportedDevice.Kinect1)]
    public class Kinect1EditorUsersRadar : Kinect1EditorViewer
    {
        private List<Vector2> userPositions = new List<Vector2>();
        private Vector2 RadarRealWorldDimensions = new Vector2(4000, 4000);
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

        public override void Update(ZigEditorInput instance, int mainUserId)
        {
            if (!instance.ReaderInited)
            {
                return;
            }

            
            userPositions.Clear();

            foreach (ZigTrackedUser currentUser in instance.TrackedUsers.Values)
            {
                // normalize the center of mass to radar dimensions
                Vector3 com = currentUser.Position;
                Vector2 radarPosition = new Vector2(com.x / RadarRealWorldDimensions.x, -com.z / RadarRealWorldDimensions.y);

                // X axis: 0 in real world is actually 0.5 in radar units (middle of field of view)
                radarPosition.x += 0.5f;

                // clamp
                radarPosition.x = Mathf.Clamp(radarPosition.x, 0.0f, 1.0f);
                radarPosition.y = Mathf.Clamp(radarPosition.y, 0.0f, 1.0f);

                userPositions.Add(radarPosition);
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
    }
}