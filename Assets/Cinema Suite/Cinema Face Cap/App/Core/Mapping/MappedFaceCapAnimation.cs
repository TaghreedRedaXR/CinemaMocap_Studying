using System.Collections.Generic;

namespace CinemaSuite.CinemaFaceCap.App.Core.Mapping
{
    public class MappedFaceCapAnimation
    {
        private List<MappedFaceCapKeyframe> keyframes = new List<MappedFaceCapKeyframe>();

        public List<MappedFaceCapKeyframe> Keyframes
        {
            get
            {
                return keyframes;
            }
        }

        public void AddKeyframe(MappedFaceCapKeyframe keyframe)
        {
            keyframes.Add(keyframe);
        }

    }
}