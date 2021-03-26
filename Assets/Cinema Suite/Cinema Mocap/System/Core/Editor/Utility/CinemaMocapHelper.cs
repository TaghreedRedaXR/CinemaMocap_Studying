using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using BaseSystem = System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Windows.Kinect;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.Utility
{
    public enum ZigResolution
    {
        QQVGA_160_x_120,
        QVGA_320_x_240,
        VGA_640_x_480,
    }

    public class ResolutionData
    {
        protected ResolutionData(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public static ResolutionData FromZigResolution(ZigResolution res)
        {
            switch (res)
            {
                default: //fallthrough - default to QQVGA
                case ZigResolution.QQVGA_160_x_120:
                    return new ResolutionData(160, 120);
                case ZigResolution.QVGA_320_x_240:
                    return new ResolutionData(320, 240);
                case ZigResolution.VGA_640_x_480:
                    return new ResolutionData(640, 480);
            }
        }
    }

    internal class CinemaMocapHelper
    {
        private static readonly ZigJointId[] ZigJointParents =
        {
            ZigJointId.None,
            ZigJointId.Neck,
            ZigJointId.Torso,
            ZigJointId.Waist,
            ZigJointId.None,
            ZigJointId.Neck,
            ZigJointId.Neck,
            ZigJointId.LeftShoulder,
            ZigJointId.LeftElbow,
            ZigJointId.LeftWrist,
            ZigJointId.LeftHand,
            ZigJointId.Neck,
            ZigJointId.Neck,
            ZigJointId.RightShoulder,
            ZigJointId.RightElbow,
            ZigJointId.RightWrist,
            ZigJointId.RightHand,
            ZigJointId.Waist,
            ZigJointId.LeftHip,
            ZigJointId.LeftKnee,
            ZigJointId.LeftAnkle,
            ZigJointId.Waist,
            ZigJointId.RightHip,
            ZigJointId.RightKnee,
            ZigJointId.RightAnkle
        };

        public static ZigJointId ParentBoneJoint(ZigJointId joint)
        {
            return ZigJointParents[(int)joint];
        }

        private static readonly JointType[] JointParents =
        {
            // PARENT               // CHILD
            JointType.SpineBase,    // SpineBase     0
            JointType.SpineBase,    // SpineMid      1
            JointType.SpineShoulder,// Neck          2
            JointType.Neck,         // Head          3
            JointType.SpineShoulder,// ShoulderLeft  4
            JointType.ShoulderLeft, // ElbowLeft     5
            JointType.ElbowLeft,    // WristLeft     6
            JointType.WristLeft,    // HandLeft      7
            JointType.SpineShoulder,// ShoulderRight 8
            JointType.ShoulderRight,// ElbowRight    9
            JointType.ElbowRight,   // WristRight   10
            JointType.WristRight,   // HandRight    11
            JointType.SpineBase,    // HipLeft      12
            JointType.HipLeft,      // KneeLeft     13
            JointType.KneeLeft,     // AnkleLeft    14
            JointType.AnkleLeft,    // FootLeft     15
            JointType.SpineBase,    // HipRight     16
            JointType.HipRight,     // KneeRight    17
            JointType.KneeRight,    // AnkleRight   18
            JointType.AnkleRight,   // FootRight    19
            JointType.SpineMid,     // SpineShoulder20
            JointType.HandLeft,     // HandTipLeft  21
            JointType.WristLeft,     // ThumbLeft    22
            JointType.HandRight,    // HandTipRight 23
            JointType.WristRight     // ThumbRight   24
        };

        public static JointType ParentBoneJoint(JointType joint)
        {
            return JointParents[(int)joint];
        }

        public static void drawFastBox(Color32[] colArray, int w, int h, int x1, int y1, int x2, int y2, Color32 col)
        {
            for (int y = y1; y <= y2; y++)
            {
                for (int x = x1; x <= x2; x++)
                {
                    int i = x + y * w;
                    if (x >= 0 && y >= 0 && x < w && y < h && i < colArray.Length)
                    {
                        colArray[i] = col;
                    }
                }
            }
        }

        // Draws a line between two points (joints) using the Bresenham Algorithm: http://tech-algorithm.com/articles/drawing-line-using-bresenham-algorithm/
        public static void drawFastLine(Color32[] colArray, int width, int height, int x, int y, int x2, int y2, Color32 col)
        {
            //if (!(x >= 0 && y >= 0 && x2 >= 0 && y2 >= 0 && x < width && y < height && x2 < width && y2 < height)) // ignore invalid joints! (TODO: why are they there?)
            //{
            //    return;
            //}

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

            int longest = BaseSystem.Math.Abs(w);
            int shortest = BaseSystem.Math.Abs(h);

            if (!(longest > shortest))
            {
                longest = BaseSystem.Math.Abs(h);
                shortest = BaseSystem.Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            int numerator = longest >> 1; // numerator = half of longest, but avoids being rounded at every whole number instead of halfway point.

            for (int j = 0; j <= longest; j++)
            {
                int i = x + y * width;
                if (x >= 0 && y >= 0 && x2 < width && y2 < height && i < colArray.Length) // valid index check
                {
                    colArray[i] = col;
                }

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        public static Vector3 GetVector3(CameraSpacePoint spacePoint)
        {
            return new Vector3(spacePoint.X, spacePoint.Y, -spacePoint.Z);
        }

        public static Quaternion GetQuaternion(Windows.Kinect.Vector4 vec4)
        {
            if (vec4.W == 0)
            {
                return Quaternion.identity;
            }
            return new Quaternion(vec4.X, vec4.Y, vec4.Z, vec4.W);
        }

        public static List<BaseSystem.Type> GetInputProfiles()
        {
            var profiles = new List<BaseSystem.Type>();

            foreach (BaseSystem.Type type in GetAllSubTypes(typeof(InputProfile)))
            {
                foreach (InputProfileAttribute attribute in type.GetCustomAttributes(typeof(InputProfileAttribute), true))
                {
                    if (attribute != null)
                    {
                        profiles.Add(type);
                    }
                }
            }
            return profiles;
        }

        public static List<BaseSystem.Type> GetOutputProfiles()
        {
            var profiles = new List<BaseSystem.Type>();

            foreach (BaseSystem.Type type in GetAllSubTypes(typeof(OutputProfile)))
            {
                foreach (OutputProfileAttribute attribute in type.GetCustomAttributes(typeof(OutputProfileAttribute), true))
                {
                    if (attribute != null)
                    {
                        profiles.Add(type);
                    }
                }
            }
            return profiles;
        }

        public static List<BaseSystem.Type> GetMappingProfiles()
        {
            List<BaseSystem.Type> profiles = new List<BaseSystem.Type>();

            foreach (BaseSystem.Type type in GetAllSubTypes(typeof(MappingProfile)))
            {
                foreach (var attribute in type.GetCustomAttributes(typeof(MappingProfileAttribute), true))
                {
                    if (attribute != null)
                    {
                        profiles.Add(type);
                    }
                }
            }
            return profiles;
        }

        internal static List<BaseSystem.Type> GetLayoutProfiles()
        {
            List<BaseSystem.Type> profiles = new List<BaseSystem.Type>();

            foreach (BaseSystem.Type type in GetAllSubTypes(typeof(CinemaMocapLayout)))
            {
                foreach (CinemaMocapLayoutAttribute attribute in type.GetCustomAttributes(typeof(CinemaMocapLayoutAttribute), true))
                {
                    if (attribute != null)
                    {
                        profiles.Add(type);
                    }
                }
            }
            return profiles;
        }

        internal static List<BaseSystem.Type> GetFilters()
        {
            List<BaseSystem.Type> profiles = new List<BaseSystem.Type>();

            foreach (BaseSystem.Type type in GetAllSubTypes(typeof(MocapFilter)))
            {
                foreach (MocapFilterAttribute attribute in type.GetCustomAttributes(typeof(MocapFilterAttribute), true))
                {
                    if (attribute != null)
                    {
                        profiles.Add(type);
                    }
                }
            }
            return profiles;
        }

        /// <summary>
        /// Get all Sub types from the given parent type.
        /// </summary>
        /// <param name="ParentType">The parent type</param>
        /// <returns>all children types of the parent.</returns>
        public static BaseSystem.Type[] GetAllSubTypes(BaseSystem.Type ParentType)
        {
            List<BaseSystem.Type> list = new List<BaseSystem.Type>();
            foreach (Assembly a in BaseSystem.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (BaseSystem.Type type in a.GetTypes())
                {
                    if (type.IsSubclassOf(ParentType))
                    {
                        list.Add(type);
                    }
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// If there is a name conflict. Iterate until we find a new name.
        /// </summary>
        /// <param name="fileName">The original filename</param>
        /// <returns>The new filename.</returns>
        public static string GetNewFilename(string folder, string filename, string extension)
        {
            int i = 1;
            while (BaseSystem.IO.File.Exists(string.Format("{0}/{1}{2}.{3}", folder, filename, i, extension)))
            {
                i++;
            }
            return string.Format("{0}{1}", filename, i);
        }

        /// <summary>
        /// Gets the path of a folder or file in the project relative to the project's assets folder.
        /// </summary>
        /// <param name="aabsolutePath">The full path of the folder or file.</param>
        /// <returns>The path relative to the project assets folder.</returns>
        public static string GetRelativeProjectPath(string absolutePath)
        {
            string projectAbsPath = (Application.dataPath).Replace("Assets", "");
            return absolutePath.Replace(projectAbsPath, "");
        }
    }
}