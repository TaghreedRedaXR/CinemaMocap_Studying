using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.UI
{
    /// <summary>
    /// Attribute for Mocap Layouts
    /// </summary>
    public class CinemaMocapLayoutAttribute : Attribute
    {
        // User friendly name
        private string profileName = "LayoutProfile";

        // For ordering
        private int ordinal = 0;

        public CinemaMocapLayoutAttribute(string profileName, int ordinal)
        {
            this.profileName = profileName;
            this.ordinal = ordinal;
        }

        /// <summary>
        /// The name of the Mocap Layout.
        /// </summary>
        public string Name
        {
            get
            {
                return profileName;
            }
        }

        /// <summary>
        /// The Ordinal of the Layout.
        /// </summary>
        public int Ordinal
        {
            get
            {
                return ordinal;
            }
        }
    }

    /// <summary>
    /// A Parent class for creating custom layouts for the Cinema Mocap window.
    /// </summary>
    public abstract class CinemaMocapLayout
    {
        /// <summary>
        /// The default aspect ratio of the current sensor. Use this value to optimize your layout.
        /// </summary>
        public float AspectRatio = 1f;

        // The amount of Viewers in this layout.
        protected int viewerCount = 1;

        protected static int GAP = 4;

        private Rect area;

        /// <summary>
        /// Get a List of Viewer Rects based on the set Area.
        /// </summary>
        /// <returns>A list of Rects for each viewer.</returns>
        public List<Rect> GetViewerRects()
        {
            return GetViewerRects(this.area);
        }

        /// <summary>
        /// Override this Method for your custom layout.
        /// </summary>
        /// <param name="mainSpace">The main space to be split up.</param>
        /// <returns>A list of Rects corresponding to each individual space.</returns>
        public virtual List<Rect> GetViewerRects(Rect area)
        {
            List<Rect> viewerSpaces = new List<Rect>();
            viewerSpaces.Add(area);
            return viewerSpaces;
        }

        /// <summary>
        /// Load all of the layout profiles found in the project.
        /// </summary>
        public static List<TypeLabelContextData> LoadMetaData()
        {
            List<TypeLabelContextData> layoutProfiles = new List<TypeLabelContextData>();

            List<Type> types = CinemaMocapHelper.GetLayoutProfiles();
            foreach (Type t in types)
            {
                foreach (CinemaMocapLayoutAttribute attribute in t.GetCustomAttributes(typeof(CinemaMocapLayoutAttribute), true))
                {
                    layoutProfiles.Add(new TypeLabelContextData(t, attribute.Name, attribute.Ordinal));
                }
            }

            layoutProfiles.Sort(delegate(TypeLabelContextData x, TypeLabelContextData y)
            {
                return x.Ordinal - y.Ordinal;
            });

            return layoutProfiles;
        }

        /// <summary>
        /// The amount of viewers in this layout.
        /// </summary>
        public int ViewerCount
        {
            get { return viewerCount; }
        }

        /// <summary>
        /// The total Area to be divided up.
        /// </summary>
        public Rect Area
        {
            get { return area; }
            set { area = value; }
        }

        internal Rect GetMainViewer()
        {
            return GetViewerRects()[0];
        }
    }
}
