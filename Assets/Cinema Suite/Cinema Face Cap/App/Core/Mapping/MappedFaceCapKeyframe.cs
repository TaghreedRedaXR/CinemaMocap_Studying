
namespace CinemaSuite.CinemaFaceCap.App.Core.Mapping
{
    public class MappedFaceCapKeyframe
    {
        private MappedFaceCapFrame frame;
        private float elapsedTime;

        public MappedFaceCapKeyframe(MappedFaceCapFrame frame, float elapsedTime)
        {
            this.frame = frame;
            this.elapsedTime = elapsedTime;
        }

        public MappedFaceCapFrame Frame
        {
            get
            {
                return frame;
            }
        }

        public float ElapsedTime
        {
            get
            {
                return elapsedTime;
            }
        }
    }
}
