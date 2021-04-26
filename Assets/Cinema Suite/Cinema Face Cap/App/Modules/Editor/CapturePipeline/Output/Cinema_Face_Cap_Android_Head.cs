using System;
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline.Output 
{
	[Name("Android Head")]
	public class Cinema_Face_Cap_Android_Head : StandardOutputFace 
	{
		public override FaceStructure GetTargetStructure()
		{
			var faceStructure = new FaceStructure("Cinema_Face_Cap_Android_Head");
			faceStructure.OrientationNodePath = "Head";
			faceStructure.FacePath = "Head.Face";
			faceStructure.Add("blendShape1.JawOpen", FaceShapeAnimations.JawOpen, x => x * 100);
			faceStructure.Add("blendShape1.LipCornerDepressorLeft", FaceShapeAnimations.LipCornerDepressorLeft, x => x * 100);
			faceStructure.Add("blendShape1.LipCornerDepressorRight", FaceShapeAnimations.LipCornerDepressorRight, x => x * 100);
			faceStructure.Add("blendShape1.LipCornerPullerLeft", FaceShapeAnimations.LipCornerPullerLeft, x => x * 100);
			faceStructure.Add("blendShape1.LipCornerPullerRight", FaceShapeAnimations.LipCornerPullerRight, x => x * 100);
			faceStructure.Add("blendShape1.LipPucker", FaceShapeAnimations.LipPucker, x => x * 100);
			faceStructure.Add("blendShape1.LipStretcherLeft", FaceShapeAnimations.LipStretcherLeft, x => x * 100);
			faceStructure.Add("blendShape1.LipStretcherRight", FaceShapeAnimations.LipStretcherRight, x => x * 100);
			faceStructure.Add("blendShape1.LowerLipDepressorLeft", FaceShapeAnimations.LowerlipDepressorLeft, x => x * 100);
			faceStructure.Add("blendShape1.LowerLipDepressorRight", FaceShapeAnimations.LowerlipDepressorRight, x => x * 100);
			faceStructure.Add("blendShape1.RightEyeClosed", FaceShapeAnimations.RightEyeClosed, x => x * 100);
			faceStructure.Add("blendShape1.LeftEyeClosed", FaceShapeAnimations.LeftEyeClosed, x => x * 100);
			faceStructure.Add("blendShape1.RightCheekPuff", FaceShapeAnimations.RightCheekPuff, x => x * 100);
			faceStructure.Add("blendShape1.LeftCheekPuff", FaceShapeAnimations.LeftCheekPuff, x => x * 100);
			faceStructure.Add("blendShape1.LeftEyebrowLowerer", FaceShapeAnimations.LefteyebrowLowerer, x => Math.Max(x, 0f) * 100f);
			faceStructure.Add("blendShape1.RightEyebrowLowerer", FaceShapeAnimations.RighteyebrowLowerer, x => Math.Max(x, 0f) * 100f);
			faceStructure.Add("blendShape1.LeftEyebrowRaise", FaceShapeAnimations.LefteyebrowLowerer, x => Math.Min(x, 0f) * -100f);
			faceStructure.Add("blendShape1.RightEyebrowRaise", FaceShapeAnimations.RighteyebrowLowerer, x => Math.Min(x, 0f) * -100f);
			faceStructure.Add("blendShape1.JawSlideLeft", FaceShapeAnimations.JawSlideRight, x => Math.Min(x, 0f) * -100f);
			faceStructure.Add("blendShape1.JawSlideRight", FaceShapeAnimations.JawSlideRight, x => Math.Max(x, 0f) * 100f);
			return faceStructure;
		}
	}
}