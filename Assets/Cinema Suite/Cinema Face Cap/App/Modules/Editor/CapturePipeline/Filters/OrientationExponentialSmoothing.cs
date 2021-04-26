
using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using UnityEditor;
using UnityEngine;

[NameAttribute("Orientation Smoothing")]
[CaptureFilterAttribute(true)]
[Ordinal(10)]
public class OrientationExponentialSmoothing : CaptureFilter
{
    private float Smoothing = 0.75f;
    private float JitterRadius = 3f;

    public const string SMOOTHING_KEY = "CinemaSuite.FaceCap.OrientationExponentialSmoothingFilter.Smoothing";
    public const string JITTER_KEY = "CinemaSuite.FaceCap.OrientationExponentialSmoothingFilter.JitterValue";

    public override string ENABLED_KEY
    {
        get
        {
            return "CinemaSuite.FaceCap.OrientationExponentialSmoothingFilter.Enabled";
        }
    }

    public override string ORDINAL_KEY
    {
        get
        {
            return "CinemaSuite.FaceCap.OrientationExponentialSmoothingFilter.Ordinal";
        }
    }

    public OrientationExponentialSmoothing()
    {
        base.Enabled = true;

        if (EditorPrefs.HasKey(SMOOTHING_KEY))
        {
            Smoothing = EditorPrefs.GetFloat(SMOOTHING_KEY);
        }
        else
        {
            EditorPrefs.SetFloat(SMOOTHING_KEY, Smoothing);
        }

        if (EditorPrefs.HasKey(JITTER_KEY))
        {
            JitterRadius = EditorPrefs.GetFloat(JITTER_KEY);
        }
        else
        {
            EditorPrefs.SetFloat(JITTER_KEY, JitterRadius);
        }


        if (EditorPrefs.HasKey(ENABLED_KEY))
        {
            Enabled = EditorPrefs.GetBool(ENABLED_KEY);
        }
        else
        {
            EditorPrefs.SetBool(ENABLED_KEY, Enabled);
        }

        if (EditorPrefs.HasKey(ORDINAL_KEY))
        {
            Ordinal = EditorPrefs.GetInt(ORDINAL_KEY);
        }
        else
        {
            EditorPrefs.SetInt(ORDINAL_KEY, Ordinal);
        }
    }

    public override bool UpdateParameters()
    {
        var result = false;

        EditorGUI.indentLevel++;
        var tempSmoothing = EditorGUILayout.Slider(new GUIContent("Smoothing", "A smoothing factor. A value of 1 will use a moving average of the previous frames, a value of 0 will use the current frame's value."), Smoothing, 0f, 1f);
        if (tempSmoothing != Smoothing)
        {
            Smoothing = tempSmoothing;
            if (EditorGUIUtility.hotControl == 0) // Don't fire an update until the user has unclicked the slider.
            {
                result = true;
            }

            EditorPrefs.SetFloat(SMOOTHING_KEY, Smoothing);
        }

        var tempJitterRadius = EditorGUILayout.Slider(new GUIContent("Jitter Degrees", "When a difference in orientation between frames is less than this value, the movement will be reduced to help reduce jitter."), JitterRadius, 0f, 90f);
        if (tempJitterRadius != JitterRadius)
        {
            JitterRadius = tempJitterRadius;
            if (EditorGUIUtility.hotControl == 0) // Don't fire an update until the user has unclicked the slider.
            {
                result = true;
            }

            EditorPrefs.SetFloat(JITTER_KEY, JitterRadius);
        }

        EditorGUI.indentLevel--;

        return result;
    }

    public override MappedFaceCapFrame Filter(CaptureCache cache)
    {
        var output = new MappedFaceCapFrame(cache.CurrentFace);
        var cacheAfter = cache.GetCacheAfterFilter(this.Name);

        if (cacheAfter.Count != 0)
        {
            var lastFiltered = cacheAfter[cacheAfter.Count - 1];

            Quaternion filteredOrientation;
            float angleDiff = Mathf.Abs(Quaternion.Angle(lastFiltered.rotation, output.rotation));
                
            if (angleDiff < JitterRadius)
            {
                filteredOrientation = Quaternion.Slerp(lastFiltered.rotation, output.rotation, angleDiff / JitterRadius);
            }
            else
            {
                filteredOrientation = output.rotation;
            }

            output.rotation = Quaternion.Lerp(filteredOrientation, lastFiltered.rotation, Smoothing);
        }
        
        return output;
    }
}
