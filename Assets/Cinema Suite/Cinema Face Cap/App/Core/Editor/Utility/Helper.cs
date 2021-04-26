using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Windows.Kinect;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility
{
    public class Helper
    {
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

            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);

            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
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

        public static List<Type> GetInputProfiles()
        {
            List<Type> profiles = new List<Type>();

            foreach (Type type in GetAllSubTypes(typeof(InputProfile)))
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

        public static List<Type> GetOutputProfiles()
        {
            List<Type> profiles = new List<Type>();

            foreach (Type type in GetAllSubTypes(typeof(OutputProfile)))
            {
                profiles.Add(type);
            }
            return profiles;
        }

        public static List<Type> GetMappingProfiles()
        {
            List<Type> profiles = new List<Type>();

            foreach (Type type in GetAllSubTypes(typeof(MappingProfile)))
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

        internal static List<Type> GetLayoutProfiles()
        {
            List<Type> profiles = new List<Type>();

            foreach (Type type in GetAllSubTypes(typeof(CinemaFaceCapLayout)))
            {
                foreach (CinemaFaceCapLayoutAttribute attribute in type.GetCustomAttributes(typeof(CinemaFaceCapLayoutAttribute), true))
                {
                    if (attribute != null)
                    {
                        profiles.Add(type);
                    }
                }
            }
            return profiles;
        }

        internal static List<Type> GetFilters()
        {
            List<Type> profiles = new List<Type>();

            foreach (Type type in GetAllSubTypes(typeof(CaptureFilter)))
            {
                foreach (CaptureFilterAttribute attribute in type.GetCustomAttributes(typeof(CaptureFilterAttribute), true))
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
        public static Type[] GetAllSubTypes(Type ParentType)
        {
            List<Type> list = new List<Type>();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in a.GetTypes())
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
            while (File.Exists(string.Format("{0}/{1}{2}.{3}", folder, filename, i, extension)))
            {
                i++;
            }
            return string.Format("{0}{1}", filename, i);
        }
    }
}