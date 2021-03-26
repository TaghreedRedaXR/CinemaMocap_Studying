
using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline
{
    public class MappingProfileAttribute : Attribute
    {
        private string name;
        private InputSkeletonType inputSkeleton;
        private Type outputProfileType;

        public MappingProfileAttribute(string name, InputSkeletonType inputSkeleton, Type outputProfileType)
        {
            this.name = name;
            this.inputSkeleton = inputSkeleton;
            this.outputProfileType = outputProfileType;
        }

        public string Name
        {
            get { return name; }
        }

        public InputSkeletonType InputSkeleton
        {
            get { return inputSkeleton; }
        }

        public Type OutputType
        {
            get { return outputProfileType; }
        }
    }

    [Flags]
    public enum SkeletonMask
    {
        Ankles = 0x0001,
        Knees = 0x0002,
        Hips = 0x0004,
        Spine = 0x0008,
        Neck = 0x0010,
        Shoulders = 0x0020,
        Elbows = 0x0040,
        Wrists = 0x0080,
        Hands = 0x0100,
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

    /// <summary>
    /// Profile for mapping from an InputProfile to an OutputProfile.
    /// </summary>
    public abstract class MappingProfile
    {
        public SkeletonMask Mask = (SkeletonMask)0xFFFF;

        public NUISkeleton OutputStructure { get; set; }

        public abstract Vector3 GetHipPosition(NUISkeleton skeleton);

        public abstract NUISkeleton MapSkeleton(NUISkeleton skeleton);

        public abstract NUIHumanoidAnimation MapAnimation(NUIHumanoidAnimation animation);

        protected bool isJointMasked(NUIJointType jointType)
        {
            var result = false;

            if (jointType == NUIJointType.AnkleLeft || jointType == NUIJointType.AnkleRight)
            {
                if ((Mask & SkeletonMask.Ankles) == 0)
                    result = true;
            }
            else if (jointType == NUIJointType.KneeLeft || jointType == NUIJointType.KneeRight)
            {
                if ((Mask & SkeletonMask.Knees) == 0) { result = true; }
            }
            else if (jointType == NUIJointType.HipLeft || jointType == NUIJointType.HipRight)
            {
                if ((Mask & SkeletonMask.Hips) == 0) { result = true; }
            }
            else if (jointType == NUIJointType.ShoulderRight || jointType == NUIJointType.ShoulderLeft)
            {
                if ((Mask & SkeletonMask.Shoulders) == 0) { result = true; }
            }
            else if (jointType == NUIJointType.ElbowLeft || jointType == NUIJointType.ElbowRight)
            {
                if ((Mask & SkeletonMask.Elbows) == 0) { result = true; }
            }
            else if (jointType == NUIJointType.WristLeft || jointType == NUIJointType.WristRight)
            {
                if ((Mask & SkeletonMask.Wrists) == 0) { result = true; }
            }
            else if (jointType == NUIJointType.HandLeft || jointType == NUIJointType.HandRight)
            {
                if ((Mask & SkeletonMask.Hands) == 0) { result = true; }
            }
            else if (jointType == NUIJointType.SpineMid || jointType == NUIJointType.SpineShoulder)
            {
                if ((Mask & SkeletonMask.Spine) == 0) { result = true; }
            }
            else if (jointType == NUIJointType.Neck)
            {
                if ((Mask & SkeletonMask.Neck) == 0) { result = true; }
            }

            return result;
        }

        public virtual bool UpdateParameters()
        {
            var result = false;

            var temp = (SkeletonMask)EditorGUILayout.EnumMaskField("Mask", Mask);
            if(temp != Mask)
            {
                Mask = temp;
                result = true;
            }

            return result;
        }

        public static List<MappingProfile> LoadMappingProfiles(InputSkeletonType inputSkeleton, Type OutputProfile)
        {
            List<MappingProfile> mappingProfiles = new List<MappingProfile>();

            List<Type> types = CinemaMocapHelper.GetMappingProfiles();
            foreach (Type t in types)
            {
                foreach (MappingProfileAttribute attribute in t.GetCustomAttributes(typeof(MappingProfileAttribute), true))
                {
                    if(attribute.InputSkeleton == inputSkeleton && attribute.OutputType == OutputProfile)
                    {
                        mappingProfiles.Add(Activator.CreateInstance(t) as MappingProfile);
                    }
                }
            }

            return mappingProfiles;
        }

        public static List<MappingProfile> LoadMappingProfiles(InputSkeletonType inputSkeleton)
        {
            List<MappingProfile> mappingProfiles = new List<MappingProfile>();

            List<Type> types = CinemaMocapHelper.GetMappingProfiles();
            foreach (Type t in types)
            {
                foreach (MappingProfileAttribute attribute in t.GetCustomAttributes(typeof(MappingProfileAttribute), true))
                {
                    if (attribute.InputSkeleton == inputSkeleton)
                    {
                        mappingProfiles.Add(Activator.CreateInstance(t) as MappingProfile);
                    }
                }
            }

            return mappingProfiles;
        }

        public static List<MappingProfileMetaData> LoadMetaData(InputSkeletonType inputSkeleton)
        {
            var metaData = new List<MappingProfileMetaData>();

            List<Type> types = CinemaMocapHelper.GetMappingProfiles();
            foreach (Type t in types)
            {
                foreach (MappingProfileAttribute attribute in t.GetCustomAttributes(typeof(MappingProfileAttribute), true))
                {
                    if (attribute.InputSkeleton == inputSkeleton)
                    {
                        metaData.Add(new MappingProfileMetaData(t, attribute));
                    }
                }
            }

            return metaData;
        }
    }
}
