using System;
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline.Output 
{
	[Name("Android")]
	public class Cinema_Face_Cap_Android : StandardOutputFace 
	{
		public override FaceStructure GetTargetStructure()
		{
			var faceStructure = new FaceStructure("Cinema_Face_Cap_Android");
			faceStructure.OrientationNodePath = "Root.Pelvis.Spine_02.spine_02.Spine_03.Spine_04.Neck 1";
			faceStructure.FacePath = "FaceCapRobot.MocapBot.head.face";
			faceStructure.Add("Face_Blendshape.JawOpen", FaceShapeAnimations.JawOpen, x => x * 100);
			faceStructure.Add("Face_Blendshape.LipPucker", FaceShapeAnimations.LipPucker, x => x * 100);
			faceStructure.Add("Face_Blendshape.JawSlideRight", FaceShapeAnimations.JawSlideRight, x => Math.Max(x, 0f) * 100f);
			faceStructure.Add("Face_Blendshape.JawSlideLeft", FaceShapeAnimations.JawSlideRight, x => Math.Min(x, 0f) * -100f);
			faceStructure.Add("Face_Blendshape.LipStretcherRight", FaceShapeAnimations.LipStretcherRight, x => x * 100);
			faceStructure.Add("Face_Blendshape.LipStretcherLeft", FaceShapeAnimations.LipStretcherLeft, x => x * 100);
			faceStructure.Add("Face_Blendshape.LipCornerPullerLeft", FaceShapeAnimations.LipCornerPullerLeft, x => x * 100);
			faceStructure.Add("Face_Blendshape.LipCornerPullerRight", FaceShapeAnimations.LipCornerPullerRight, x => x * 100);
			faceStructure.Add("Face_Blendshape.LipCornerDepressorLeft", FaceShapeAnimations.LipCornerDepressorLeft, x => x * 100);
			faceStructure.Add("Face_Blendshape.LipCornerDepressorRight", FaceShapeAnimations.LipCornerDepressorRight, x => x * 100);
			faceStructure.Add("Face_Blendshape.LeftCheekPuff", FaceShapeAnimations.LeftCheekPuff, x => x * 100);
			faceStructure.Add("Face_Blendshape.RightCheekPuff", FaceShapeAnimations.RightCheekPuff, x => x * 100);
			faceStructure.Add("Face_Blendshape.LeftEyeClosed", FaceShapeAnimations.LeftEyeClosed, x => x * 100);
			faceStructure.Add("Face_Blendshape.RightEyeClosed", FaceShapeAnimations.RightEyeClosed, x => x * 100);
			faceStructure.Add("Face_Blendshape.LefteyebrowRaise", FaceShapeAnimations.LefteyebrowLowerer, x => Math.Min(x, 0f) * -100f);
			faceStructure.Add("Face_Blendshape.RighteyebrowRaise", FaceShapeAnimations.RighteyebrowLowerer, x => Math.Min(x, 0f) * -100f);
			faceStructure.Add("Face_Blendshape.LefteyebrowLowerer", FaceShapeAnimations.LefteyebrowLowerer, x => Math.Max(x, 0f) * 100f);
			faceStructure.Add("Face_Blendshape.RighteyebrowLowerer", FaceShapeAnimations.RighteyebrowLowerer, x => Math.Max(x, 0f) * 100f);
			faceStructure.Add("Face_Blendshape.LowerlipDepressorLeft", FaceShapeAnimations.LowerlipDepressorLeft, x => x * 100);
			faceStructure.Add("Face_Blendshape.LowerlipDepressorRight", FaceShapeAnimations.LowerlipDepressorRight, x => x * 100);
			return faceStructure;
		}
	}
}