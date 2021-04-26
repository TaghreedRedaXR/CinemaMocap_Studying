
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Core.Mapping
{
    public class MappedFaceCapFrame
    {
        public Dictionary<FaceShapeAnimations, float> AnimationUnits = new Dictionary<FaceShapeAnimations, float>();

        public Quaternion rotation;

        public MappedFaceCapFrame() { }

        /// <summary>
        /// Create a new instance, cloning the passed param.
        /// </summary>
        /// <param name="face"></param>
        public MappedFaceCapFrame(MappedFaceCapFrame face)
        {
            this.rotation = face.rotation;
            foreach(var au in face.AnimationUnits)
            {
                this.AnimationUnits.Add(au.Key, au.Value);
            }
        }
    }
}