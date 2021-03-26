
using UnityEditor;
using UnityEngine;
using Windows.Kinect;
namespace CinemaSuite.CinemaMocap.System.Core.Editor.UI
{
    public abstract class Kinect2EditorViewer : SensorEditorViewer
    {
        public override void Initialize()
        {
            base.Initialize();

            base.PlaceHolderImage = EditorGUIUtility.Load("Cinema Suite/Cinema Mocap/" + "CinemaMocap2.png") as Texture2D;
            if (PlaceHolderImage == null)
            {
                UnityEngine.Debug.LogWarning("CinemaMocap2.png is missing from Resources folder.");
            }
        }

        /// <summary>
        /// Initialize the viewer with the existing Kinect Sensor reference.
        /// </summary>
        /// <param name="kinect2Sensor">The Kinect Sensor.</param>
        public void Initialize(KinectSensor kinect2Sensor)
        {
            Initialize();
            SetupReader(kinect2Sensor);
        }

        public abstract void SetupReader(KinectSensor kinect2Sensor);

        public abstract void Update();

        public virtual void Update(Body[] bodies, int mainUserId)
        {
            Update();
        }
    }
}
