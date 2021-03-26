using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.UI
{

    public class SensorEditorViewerAttribute : Attribute
    {
        private string name = "SensorViewer";
        private SupportedDevice device = SupportedDevice.Kinect1;

        /// <summary>
        /// Try to give each viewer a unique Id.
        /// </summary>
        private int ordinal = 1;

        public SensorEditorViewerAttribute(string name, int ordinal, SupportedDevice device)
        {
            this.name = name;
            this.device = device;
            this.ordinal = ordinal;
        }

        /// <summary>
        /// The user friendly name of the attribute.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// The ordinal of the viewer. Used to prioritize certain viewers over others.
        /// </summary>
        public int Ordinal
        {
            get
            {
                return ordinal;
            }
        }

        /// <summary>
        /// The device that this viewer is designed for.
        /// </summary>
        public SupportedDevice Device
        {
            get
            {
                return device;
            }
        }
    }

    /// <summary>
    /// Parent class for Viewers.
    /// </summary>
    public abstract class SensorEditorViewer
    {
        protected Texture2D PlaceHolderImage;
        protected float aspectRatio = 4 / 3f;

        protected Rect area;
        protected Rect ToolbarArea;
        protected Rect selectorArea;
        protected Rect toolbarLeftover;
        protected Rect ContentBackgroundArea;

        /// <summary>
        /// Initialize the viewer
        /// </summary>
        public virtual void Initialize()
        {
            PlaceHolderImage = EditorGUIUtility.Load("Cinema Suite/Cinema Mocap/" + "CinemaMocap.png") as Texture2D;
            if (PlaceHolderImage == null)
            {
                UnityEngine.Debug.LogWarning("CinemaMocap.png is missing from Resources folder.");
            }
        }

        /// <summary>
        /// Override to add custom controls to the viewer toolbar.
        /// </summary>
        /// <param name="area">The area of the toolbar.</param>
        public virtual void UpdateToolbar()
        { }

        public void DrawBackground()
        {
			//EditorStyles.helpBox
            GUI.Box(area, string.Empty, "helpBox");
            GUI.Box(ToolbarArea, string.Empty, EditorStyles.toolbar);
        }

        public virtual void DrawContent()
        { }

        /// <summary>
        /// Draw an image as a place holder while the device is not on.
        /// </summary>
        public void DrawPlaceHolder()
        {
            float textureHeight = ContentBackgroundArea.height;
            float textureWidth = textureHeight * aspectRatio;

            if ((ContentBackgroundArea.width / ContentBackgroundArea.height) < aspectRatio)
            {
                textureWidth = ContentBackgroundArea.width;
                textureHeight = textureWidth * (1f / aspectRatio);
            }
            Rect areaContent = new Rect((ContentBackgroundArea.x + (ContentBackgroundArea.width - textureWidth) / 2) + 2, (ContentBackgroundArea.y + (ContentBackgroundArea.height - textureHeight) / 2) + 2, textureWidth - 4, textureHeight - 4);

            GUI.DrawTexture(areaContent, PlaceHolderImage);
        }

        public virtual void Destroy()
        {
        }

        public Rect Area
        {
            get { return area; }
            set 
            { 
                area = value;

                ToolbarArea = new Rect(area.x, area.y, area.width, 17);
                ContentBackgroundArea = new Rect(area.x, area.y + 17, area.width, area.height - 17);
                selectorArea = new Rect(area.x + 5, area.y, 100f, 17f);
                toolbarLeftover = new Rect(area.x + selectorArea.width, area.y, area.width - selectorArea.width, 17);
            }
        }

        public Rect SelectorArea
        {
            get { return selectorArea; }
        }

        internal static List<Type> GetSensorEditorViewers(params SupportedDevice[] devices)
        {
            List<Type> viewers = new List<Type>();

            foreach (Type type in CinemaMocapHelper.GetAllSubTypes(typeof(SensorEditorViewer)))
            {
                foreach (SensorEditorViewerAttribute attribute in type.GetCustomAttributes(typeof(SensorEditorViewerAttribute), true))
                {
                    if (attribute != null && ArrayUtility.Contains<SupportedDevice>(devices, attribute.Device))
                    {
                        viewers.Add(type);
                    }
                }
            }
            return viewers;
        }
    }
}