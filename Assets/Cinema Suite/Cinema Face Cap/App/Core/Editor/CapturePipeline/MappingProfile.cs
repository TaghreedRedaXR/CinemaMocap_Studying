
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline
{
    [Flags]
    public enum FaceMask
    {
        JawOpen = 0x0001,
        JawSlide = 0x0002,
        LipPucker = 0x0004,
        LipStretcher = 0x0008,
        LipCornerPuller = 0x0010,
        LipCornerDepressor = 0x0020,
        LipLowerDepressor = 0x0040,
        CheekPuff = 0x0080,
        EyeClose = 0x0100,
        Eyebrows = 0x0200,
    }

    public class MappingProfileMetaData
    {
        public Type Type;
        public MappingProfileAttribute Attribute;

        public MappingProfileMetaData(Type t, MappingProfileAttribute attribute)
        {
            this.Type = t;
            this.Attribute = attribute;
        }
    }
    
    public class MappingProfileAttribute : Attribute
    {
        private string name;
        private InputFace inputFace;
        private Type outputProfileType;

        public MappingProfileAttribute(string name, InputFace inputFace, Type outputProfileType)
        {
            this.name = name;
            this.inputFace = inputFace;
            this.outputProfileType = outputProfileType;
        }

        public string Name
        {
            get { return name; }
        }

        public InputFace InputFace
        {
            get { return inputFace; }
        }

        public Type OutputType
        {
            get { return outputProfileType; }
        }
    }

    /// <summary>
    /// Profile for mapping from an InputProfile to an OutputProfile.
    /// </summary>
    public abstract class MappingProfile
    {
        public FaceMask Mask = (FaceMask) 0xFFFF;
        public bool mapOrientation = true;

        public const string MASK_KEY = "CinemaSuite.FaceCap.Mapping.Mask";
        public const string ENABLE_ORIENTATION_KEY = "CinemaSuite.FaceCap.Mapping.EnableOrientation";

        public FaceStructure OutputStructure { get; set; }

        public MappingProfile()
        {
            if (EditorPrefs.HasKey(MASK_KEY))
            {
                Mask = (FaceMask)EditorPrefs.GetInt(MASK_KEY);
            }

            if (EditorPrefs.HasKey(ENABLE_ORIENTATION_KEY))
            {
                mapOrientation = EditorPrefs.GetBool(ENABLE_ORIENTATION_KEY);
            }
        }

        public virtual bool UpdateParameters()
        {
            var result = false;

            var temp = (FaceMask)EditorGUILayout.EnumMaskField("Mask", Mask);
            if (temp != Mask)
            {
                Mask = temp;
                result = true;

                EditorPrefs.SetInt(MASK_KEY, (int)Mask);
            }

            var tempOrientation = EditorGUILayout.Toggle(new GUIContent("Enable Orientation"), mapOrientation);
            if(tempOrientation != mapOrientation)
            {
                mapOrientation = tempOrientation;
                result = true;

                EditorPrefs.SetBool(ENABLE_ORIENTATION_KEY, mapOrientation);
            }

            return result;
        }

        public abstract MappedFaceCapFrame MapFace(MappedFaceCapFrame face);

        public static List<MappingProfile> LoadMappingProfiles(InputFace inputFace, Type OutputProfile)
        {
            List<MappingProfile> mappingProfiles = new List<MappingProfile>();

            List<Type> types = Utility.Helper.GetMappingProfiles();
            foreach (Type t in types)
            {
                foreach (MappingProfileAttribute attribute in t.GetCustomAttributes(typeof(MappingProfileAttribute), true))
                {
                    if(attribute.InputFace == inputFace && (attribute.OutputType == OutputProfile || OutputProfile.IsSubclassOf(attribute.OutputType)))
                    {
                        mappingProfiles.Add(Activator.CreateInstance(t) as MappingProfile);
                    }
                }
            }

            return mappingProfiles;
        }

        public static List<MappingProfile> LoadMappingProfiles(InputFace inputFace)
        {
            List<MappingProfile> mappingProfiles = new List<MappingProfile>();

            List<Type> types = Utility.Helper.GetMappingProfiles();
            foreach (Type t in types)
            {
                foreach (MappingProfileAttribute attribute in t.GetCustomAttributes(typeof(MappingProfileAttribute), true))
                {
                    if (attribute.InputFace == inputFace)
                    {
                        mappingProfiles.Add(Activator.CreateInstance(t) as MappingProfile);
                    }
                }
            }

            return mappingProfiles;
        }

        public static List<MappingProfileMetaData> LoadMetaData(InputFace inputFace)
        {
            var metaData = new List<MappingProfileMetaData>();

            List<Type> types = Utility.Helper.GetMappingProfiles();
            foreach (Type t in types)
            {
                foreach (MappingProfileAttribute attribute in t.GetCustomAttributes(typeof(MappingProfileAttribute), true))
                {
                    if (attribute.InputFace == inputFace)
                    {
                        metaData.Add(new MappingProfileMetaData(t, attribute));
                    }
                }
            }

            return metaData;
        }
    }
}
