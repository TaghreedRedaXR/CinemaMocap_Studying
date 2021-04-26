
using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline.Output;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using System;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline
{
    [MappingProfileAttribute("Kinect 2 HD -> Standard Face", InputFace.SeventeenAnimationUnits, typeof(StandardOutputFace))]
    public class SeventeenAUOneToOneFaceMapper : MappingProfile
    {
        public override MappedFaceCapFrame MapFace(MappedFaceCapFrame face)
        {
            var output = new MappedFaceCapFrame(face);

            // Reverse the yaw of the orientation
            if (mapOrientation)
            {
                Vector3 euler = face.rotation.eulerAngles;
                euler.x *= -1;
                output.rotation = Quaternion.Euler(euler);
            }
            else
            {
                output.rotation = Quaternion.identity;
            }

            foreach (var au in face.AnimationUnits)
            {
                if(isAnimationUnitMasked(au.Key))
                {
                    output.AnimationUnits[au.Key] = 0;
                }
            }

            return output;
        }

        private bool isAnimationUnitMasked(FaceShapeAnimations key)
        {
            var result = false;

            if (key == FaceShapeAnimations.JawOpen)
            {
                if ((Mask & FaceMask.JawOpen) == 0)
                    result = true;
            }
            else if (key == FaceShapeAnimations.JawSlideRight)
            {
                if ((Mask & FaceMask.JawSlide) == 0) { result = true; }
            }
            else if (key == FaceShapeAnimations.LeftCheekPuff || key == FaceShapeAnimations.RightCheekPuff)
            {
                if ((Mask & FaceMask.CheekPuff) == 0) { result = true; }
            }
            else if (key == FaceShapeAnimations.RighteyebrowLowerer || key == FaceShapeAnimations.LefteyebrowLowerer)
            {
                if ((Mask & FaceMask.Eyebrows) == 0) { result = true; }
            }
            else if (key == FaceShapeAnimations.LeftEyeClosed || key == FaceShapeAnimations.RightEyeClosed)
            {
                if ((Mask & FaceMask.EyeClose) == 0) { result = true; }
            }
            else if (key == FaceShapeAnimations.LipCornerDepressorLeft || key == FaceShapeAnimations.LipCornerDepressorRight)
            {
                if ((Mask & FaceMask.LipCornerDepressor) == 0) { result = true; }
            }
            else if (key == FaceShapeAnimations.LipCornerPullerLeft || key == FaceShapeAnimations.LipCornerPullerRight)
            {
                if ((Mask & FaceMask.LipCornerPuller) == 0) { result = true; }
            }
            else if (key == FaceShapeAnimations.LowerlipDepressorLeft || key == FaceShapeAnimations.LowerlipDepressorRight)
            {
                if ((Mask & FaceMask.LipLowerDepressor) == 0) { result = true; }
            }
            else if (key == FaceShapeAnimations.LipPucker)
            {
                if ((Mask & FaceMask.LipPucker) == 0) { result = true; }
            }
            else if (key == FaceShapeAnimations.LipStretcherLeft || key == FaceShapeAnimations.LipStretcherRight)
            {
                if ((Mask & FaceMask.LipStretcher) == 0) { result = true; }
            }

            return result;
        }
    }
}