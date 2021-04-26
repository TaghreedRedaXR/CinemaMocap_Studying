
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[NameAttribute("Anim. Unit Amplifier")]
[CaptureFilterAttribute(true)]
[Ordinal(2)]
public class AnimationUnitAmplifier : CaptureFilter
{
    private ReorderableList reorderableList;
    private List<AmplifierOption> options = new List<AmplifierOption>();
    private float[] columns = new float[2] { 0.6f, 0.4f };

    private bool hasElementUpdated = false;

    public const string LIST_KEY = "CinemaSuite.FaceCap.AnimationUnitAmplifierFilter.List";
    public const char LIST_ITEM_DELIMITER = '_';
    public const char ITEM_VALUES_DELIMITER = '*';

    public override string ENABLED_KEY
    {
        get
        {
            return "CinemaSuite.FaceCap.AnimationUnitAmplifierFilter.Enabled";
        }
    }

    public override string ORDINAL_KEY
    {
        get
        {
            return "CinemaSuite.FaceCap.AnimationUnitAmplifierFilter.Ordinal";
        }
    }

    public AnimationUnitAmplifier()
    {
        base.Enabled = true;

        reorderableList = new ReorderableList(options, typeof(AmplifierOption), true, true, true, true);

        reorderableList.drawHeaderCallback = drawHeader;
        reorderableList.drawElementCallback = drawElement;
        
        reorderableList.onChangedCallback = SaveListAsPref;

        LoadListFromPref();


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

    private void drawHeader(Rect rect)
    {
        var rects = new Rect[2];
        float gap = 8f;
        for (int i = 0; i < rects.Length; i++)
        {
            rects[i] = new Rect(rect);
            float x = rect.x + 4;
            if (i > 0)
            {
                x = rects[i - 1].x + rects[i - 1].width;
            }
            rects[i].x = x + gap;
            rects[i].width = (rect.width - 4 - (gap * rects.Length)) * columns[i] + gap;
        }

        EditorGUI.LabelField(rects[0], "Unit");
        EditorGUI.LabelField(rects[1], "Multiplier");
    }


    private void drawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var element = reorderableList.list[index] as AmplifierOption;
        rect.y += 2;

        var rects = new Rect[2];
        float gap = 8f;
        for (int i = 0; i < rects.Length; i++)
        {
            rects[i] = new Rect(rect);
            float x = rect.x - gap;
            if (i > 0)
            {
                x = rects[i - 1].x + rects[i - 1].width;
            }
            rects[i].x = x + gap;
            rects[i].width = (rect.width - (gap * rects.Length)) * columns[i] + gap;
            rects[i].height = EditorGUIUtility.singleLineHeight;

        }
        rects[1].width -= 8;

        var tempUnit = (FaceShapeAnimations)EditorGUI.EnumPopup(rects[0], element.unit);
        if(tempUnit != element.unit)
        {
            hasElementUpdated = true;
            element.unit = tempUnit;
        }
        var tempMultiplier = EditorGUI.FloatField(rects[1], element.multiplier);
        if(tempMultiplier != element.multiplier)
        {
            hasElementUpdated = true;
            element.multiplier = tempMultiplier;
        }

        if (hasElementUpdated)
            SaveListAsPref(reorderableList);
    }

    public override bool UpdateParameters()
    {
        hasElementUpdated = false;

        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(reorderableList.GetHeight()));
        rect.x += 32f; rect.width -= 32f;
        reorderableList.DoList(rect);
        
        return hasElementUpdated;
    }

    public override MappedFaceCapFrame Filter(MappedFaceCapFrame input)
    {
        var output = new MappedFaceCapFrame(input);

        foreach(var option in options)
        {
            var value = output.AnimationUnits[option.unit];
            value = value * option.multiplier;
            output.AnimationUnits[option.unit] = value;
        }

        return output;
    }

    private class AmplifierOption
    {
        public FaceShapeAnimations unit;
        public float multiplier;
    }

    public void SaveListAsPref(ReorderableList reorderableList)
    {
        string prefValue = "";
        foreach (AmplifierOption opt in reorderableList.list)
        {
            if (prefValue != "") prefValue += LIST_ITEM_DELIMITER;
            prefValue += "" + (int)opt.unit + ITEM_VALUES_DELIMITER + opt.multiplier;
        }

        EditorPrefs.SetString(LIST_KEY, prefValue);
    }

    public void LoadListFromPref()
    {
        if (!EditorPrefs.HasKey(LIST_KEY))
        {
            EditorPrefs.SetString(LIST_KEY, "");
            return;
        }

        string prefValue = EditorPrefs.GetString(LIST_KEY);
        if (prefValue == "") return;

        string[] listItems = prefValue.Split(LIST_ITEM_DELIMITER);

        for (int i = 0; i < listItems.Length; i++)
        {
            string[] itemValues = listItems[i].Split(ITEM_VALUES_DELIMITER);

            int unit = Convert.ToInt32(itemValues[0]);
            float multiplier = Convert.ToSingle(itemValues[1]);

            AmplifierOption ao = new AmplifierOption();
            ao.unit = (FaceShapeAnimations)unit;
            ao.multiplier = multiplier;

            reorderableList.list.Add(ao);
        }
    }
}
