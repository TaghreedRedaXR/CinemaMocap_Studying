
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using System;
using UnityEditor;
using UnityEngine;

[NameAttribute("Anim. Unit Smoothing")]
[CaptureFilterAttribute(true)]
[Ordinal(5)]
public class AnimationUnitSmoothing : CaptureFilter
{
    private float Smoothing = 0.75f;
    private float JitterValue = 0.05f;

    public const string SMOOTHING_KEY = "CinemaSuite.FaceCap.AnimationUnitSmoothingFilter.Smoothing";
    public const string JITTER_KEY = "CinemaSuite.FaceCap.AnimationUnitSmoothingFilter.JitterValue";

    public override string ENABLED_KEY
    {
        get
        {
            return "CinemaSuite.FaceCap.AnimationUnitSmoothingFilter.Enabled";
        }
    }

    public override string ORDINAL_KEY
    {
        get
        {
            return "CinemaSuite.FaceCap.AnimationUnitSmoothingFilter.Ordinal";
        }
    }


    public AnimationUnitSmoothing()
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
            JitterValue = EditorPrefs.GetFloat(JITTER_KEY);
        }
        else
        {
            EditorPrefs.SetFloat(JITTER_KEY, JitterValue);
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

        var tempJitterRadius = EditorGUILayout.Slider(new GUIContent("Jitter Value", "When a difference in orientation between frames is less than this value, the movement will be reduced to help reduce jitter."), JitterValue, 0f, 1f);
        if (tempJitterRadius != JitterValue)
        {
            JitterValue = tempJitterRadius;
            if (EditorGUIUtility.hotControl == 0) // Don't fire an update until the user has unclicked the slider.
            {
                result = true;
            }

            EditorPrefs.SetFloat(JITTER_KEY, JitterValue);
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

            for(int i = 0; i < lastFiltered.AnimationUnits.Count; i++)
            {
                float filteredValued = output.AnimationUnits[(FaceShapeAnimations)i];
                float diff = Mathf.Abs(lastFiltered.AnimationUnits[(FaceShapeAnimations)i] - output.AnimationUnits[(FaceShapeAnimations)i]);

                if (diff < JitterValue)
                {
                    float diffRatio = diff / JitterValue;
                    filteredValued = ((1-diffRatio) * lastFiltered.AnimationUnits[(FaceShapeAnimations)i]) + (diffRatio * filteredValued);
                }

                output.AnimationUnits[(FaceShapeAnimations)i] = ((1-Smoothing) * filteredValued) + (Smoothing) * lastFiltered.AnimationUnits[(FaceShapeAnimations)i];
            }
        }

        return output;
    }
}
