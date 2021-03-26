
using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline
{
    [NameAttribute("Tilt Correction")]
    [MocapFilterAttribute(true)]
    [Ordinal(0)]
    public class TiltCorrectionFilter : MocapFilter
    {
        private float tilt = 0f;

        public override bool UpdateParameters()
        {
            var result = false;

            EditorGUI.indentLevel++;
            var tempTilt = EditorGUILayout.FloatField("Tilt", tilt);
            if(tempTilt != tilt)
            {
                tilt = tempTilt;
                result = true;
            }

            EditorGUI.indentLevel--;

            return result;
        }

        public override NUISkeleton Filter(NUISkeleton input)
        {
            NUISkeleton output = input.Clone();

            Vector3 spineBasePosition = output.Joints[NUIJointType.SpineBase].Position;

            foreach (var joint in output.Joints)
            {
                joint.Value.Position -= spineBasePosition;
                joint.Value.Position = Quaternion.AngleAxis(tilt, Vector3.right) * joint.Value.Position;
                joint.Value.Position += spineBasePosition;
            }

            return output;
        }
    }
}