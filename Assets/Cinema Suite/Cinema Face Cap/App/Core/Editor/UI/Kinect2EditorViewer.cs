
using Microsoft.Kinect.Face;
using Windows.Kinect;


namespace CinemaSuite.CinemaFaceCap.App.Core.Editor.UI
{
    public abstract class Kinect2EditorViewer : SensorEditorViewer
    {
        public override void Initialize()
        {
            base.Initialize();
        }
        
        public void Initialize(KinectSensor kinect2Sensor)
        {
            Initialize();
            SetupReader(kinect2Sensor);
        }

        public abstract void SetupReader(KinectSensor kinect2Sensor);

        public abstract void Update();

        public virtual void Update(FaceModel faceModel, FaceAlignment faceAlignment)
        {
            Update();
        }
    }
}
