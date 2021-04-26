
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using System;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline.Output
{
    public abstract class MixamoFuse50AUFace : StandardOutputFace
    {
        /// <summary>
        /// Describe the face structure of your model to the system.
        /// </summary>
        /// <returns>A Facial Structure object describing the face.</returns>
        public override FaceStructure GetTargetStructure()
        {
            var faceStructure = new FaceStructure("temp");
            faceStructure.OrientationNodePath = "mixamorig:Hips.mixamorig:Spine.mixamorig:Spine1.mixamorig:Spine2.mixamorig:Neck.mixamorig:Head"; // The orientation node is the root node in this case.
            faceStructure.FacePath = "Body";

            // Add the morph targets in the correct order.
            faceStructure.Add("Facial_Blends.Blink_Left", FaceShapeAnimations.LeftEyeClosed, 100f);
            faceStructure.Add("Facial_Blends.Blink_Right", FaceShapeAnimations.RightEyeClosed, 100f);

            faceStructure.Add("Facial_Blends.BrowsDown_Left", FaceShapeAnimations.LefteyebrowLowerer, value => Math.Max(value, 0f) * 100f);
            faceStructure.Add("Facial_Blends.BrowsDown_Right", FaceShapeAnimations.RighteyebrowLowerer, value => Math.Max(value, 0f) * 100f);

            faceStructure.Add("Facial_Blends.BrowsIn_Left");
            faceStructure.Add("Facial_Blends.BrowsIn_Right");

            faceStructure.Add("Facial_Blends.BrowsOuterLower_Left");
            faceStructure.Add("Facial_Blends.BrowsOuterLower_Right");

            faceStructure.Add("Facial_Blends.BrowsUp_Left", FaceShapeAnimations.LefteyebrowLowerer, value => Math.Min(value, 0f) * -100f);
            faceStructure.Add("Facial_Blends.BrowsUp_Right", FaceShapeAnimations.RighteyebrowLowerer, value => Math.Min(value, 0f) * -100f);

            faceStructure.Add("Facial_Blends.CheekPuff_Left", FaceShapeAnimations.LeftCheekPuff, 100f);
            faceStructure.Add("Facial_Blends.CheekPuff_Right", FaceShapeAnimations.RightCheekPuff, 100f);

            faceStructure.Add("Facial_Blends.EyesWide_Left");
            faceStructure.Add("Facial_Blends.EyesWide_Right");

            faceStructure.Add("Facial_Blends.Frown_Left", FaceShapeAnimations.LipCornerDepressorLeft, 100f);
            faceStructure.Add("Facial_Blends.Frown_Right", FaceShapeAnimations.LipCornerDepressorRight, 100f);

            faceStructure.Add("Facial_Blends.JawBackward");
            faceStructure.Add("Facial_Blends.JawForward");

            faceStructure.Add("Facial_Blends.JawRotateY_Left");
            faceStructure.Add("Facial_Blends.JawRotateY_Right");

            faceStructure.Add("Facial_Blends.JawRotateZ_Left");
            faceStructure.Add("Facial_Blends.JawRotateZ_Right");

            faceStructure.Add("Facial_Blends.Jaw_Down");
            faceStructure.Add("Facial_Blends.Jaw_Left", FaceShapeAnimations.JawSlideRight, value => Math.Min(value, 0f) * -100f);
            faceStructure.Add("Facial_Blends.Jaw_Right", FaceShapeAnimations.JawSlideRight, value => Math.Min(value, 0f) * 100f);
            faceStructure.Add("Facial_Blends.Jaw_Up");

            faceStructure.Add("Facial_Blends.LowerLipDown_Left", FaceShapeAnimations.LowerlipDepressorLeft, 100f);
            faceStructure.Add("Facial_Blends.LowerLipDown_Right", FaceShapeAnimations.LowerlipDepressorRight, 100f);
            faceStructure.Add("Facial_Blends.LowerLipIn");
            faceStructure.Add("Facial_Blends.LowerLipOut");

            faceStructure.Add("Facial_Blends.Midmouth_Left");
            faceStructure.Add("Facial_Blends.Midmouth_Right");

            faceStructure.Add("Facial_Blends.MouthDown");

            faceStructure.Add("Facial_Blends.MouthNarrow_Left", FaceShapeAnimations.LipPucker, 100f);
            faceStructure.Add("Facial_Blends.MouthNarrow_Right", FaceShapeAnimations.LipPucker, 100f);

            faceStructure.Add("Facial_Blends.MouthOpen", FaceShapeAnimations.JawOpen, 100f);
            faceStructure.Add("Facial_Blends.MouthUp");

            faceStructure.Add("Facial_Blends.MouthWhistle_NarrowAdjust_Left");
            faceStructure.Add("Facial_Blends.MouthWhistle_NarrowAdjust_Right");

            faceStructure.Add("Facial_Blends.NoseScrunch_Left");
            faceStructure.Add("Facial_Blends.NoseScrunch_Right");

            faceStructure.Add("Facial_Blends.Smile_Left", FaceShapeAnimations.LipCornerPullerLeft, 100f);
            faceStructure.Add("Facial_Blends.Smile_Right", FaceShapeAnimations.LipCornerPullerRight, 100f);

            faceStructure.Add("Facial_Blends.Squint_Left");
            faceStructure.Add("Facial_Blends.Squint_Right");

            faceStructure.Add("Facial_Blends.TongueUp");

            faceStructure.Add("Facial_Blends.UpperLipIn");
            faceStructure.Add("Facial_Blends.UpperLipOut");

            faceStructure.Add("Facial_Blends.UpperLipUp_Left");
            faceStructure.Add("Facial_Blends.UpperLipUp_Right");

            return faceStructure;
        }
    }
}
