using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using UnityEditor;
using UnityEngine;
using BaseSystem = System;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.Viewers
{
    /// <summary>
    /// A depth viewer that can execute as an editor extension.
    /// </summary>
    [SensorEditorViewer("Depth Viewer", 0, SupportedDevice.Kinect1)]
    public class Kinect1EditorDepthViewer : Kinect1EditorViewer
    {
        // GUI
        private Texture nodesImage = null;
        private Texture bonesImage = null;

        // Options
        private bool showNodes = true;
        private bool showBones = true;

        internal Texture2D texture;
        internal ZigResolution TextureSize = ZigResolution.QVGA_320_x_240;
        internal ResolutionData textureSize;

        public Color32 BaseColor = Color.green;
        public bool UseHistogram = true;

        float[] depthHistogramMap;
        Color32[] depthToColor;
        Color32[] outputPixels;
        public int MaxDepth = 10000;

        /// <summary>
        /// Initialize the depth viewer
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            string res_dir = "Cinema Suite/Cinema Mocap/";

            string showNodesName = EditorGUIUtility.isProSkin ? "CinemaMocap_ShowNodes" : "CinemaMocap_ShowNodes_Personal";
            nodesImage = EditorGUIUtility.Load(res_dir + showNodesName + ".png") as Texture2D;
            if (nodesImage == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("{0}.png is missing from Resources folder.", showNodesName));
            }

            string showSkeletonName = EditorGUIUtility.isProSkin ? "CinemaMocap_ShowSkeleton" : "CinemaMocap_ShowSkeleton_Personal";
            bonesImage = EditorGUIUtility.Load(res_dir + showSkeletonName + ".png") as Texture2D;
            if (bonesImage == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("{0}.png is missing from Resources folder.", showSkeletonName));
            }

            textureSize = ResolutionData.FromZigResolution(TextureSize);
            texture = new Texture2D(textureSize.Width, textureSize.Height);
            texture.wrapMode = TextureWrapMode.Clamp;

            depthHistogramMap = new float[MaxDepth];
            depthToColor = new Color32[MaxDepth];
            outputPixels = new Color32[textureSize.Width * textureSize.Height];
        }

        public override void UpdateToolbar()
        {
            Rect button1 = toolbarLeftover;
            button1.width = 24;
            button1.x = toolbarLeftover.x + toolbarLeftover.width - 58;

            Rect button2 = toolbarLeftover;
            button2.width = 24;
            button2.x = toolbarLeftover.x + toolbarLeftover.width - 29;

            Rect colorArea = button1;
            colorArea.x -= 70;
            colorArea.width = 60;

            BaseColor = EditorGUI.ColorField(colorArea, string.Empty, BaseColor);
            showNodes = GUI.Toggle(button1, showNodes, new GUIContent("", nodesImage, "Show Nodes"), EditorStyles.toolbarButton);
            showBones = GUI.Toggle(button2, showBones, new GUIContent("", bonesImage, "Show Bones"), EditorStyles.toolbarButton);
        }

        public override void Update(ZigEditorInput instance, int mainUserId)
        {
            if (instance == null || ZigEditorInput.Depth == null) return;
            if (texture == null)
            {
                Initialize();
            }

            if (UseHistogram)
            {
                UpdateHistogram(ZigEditorInput.Depth);
            }
            else
            {
                depthToColor[0] = Color.black;
                for (int i = 1; i < MaxDepth; i++)
                {
                    float intensity = 1.0f - (i / (float)MaxDepth);

                    depthToColor[i].r = (byte)(BaseColor.r * intensity);
                    depthToColor[i].g = (byte)(BaseColor.g * intensity);
                    depthToColor[i].b = (byte)(BaseColor.b * intensity);
                    depthToColor[i].a = 255;
                }
            }

            // Get the user Skeleton
            ZigInputJoint[] skeleton = null;
            if (instance.TrackedUsers.ContainsKey(mainUserId))
            {
                skeleton = instance.TrackedUsers[mainUserId].Skeleton;
            }

            UpdateTexture(ZigEditorInput.Depth, skeleton);
        }

        public override void DrawContent()
        {
            float textureHeight = ContentBackgroundArea.height;
            float textureWidth = textureHeight * aspectRatio;

            if ((ContentBackgroundArea.width / ContentBackgroundArea.height) < aspectRatio)
            {
                textureWidth = ContentBackgroundArea.width;
                textureHeight = textureWidth * (1f / aspectRatio);
            }
            Rect areaContent = new Rect((ContentBackgroundArea.x + (ContentBackgroundArea.width - textureWidth) / 2) + 2, (ContentBackgroundArea.y + (ContentBackgroundArea.height - textureHeight) / 2) + 2, textureWidth - 4, textureHeight - 4);

            GUI.DrawTexture(areaContent, texture);
        }

        /// <summary>
        /// Update the depth viewer texture.
        /// </summary>
        /// <param name="depth">The depth map</param>
        private void UpdateTexture(ZigDepth depth, ZigInputJoint[] userSkeleton)
        {
            short[] rawDepthMap = depth.data;
            int depthIndex = 0;
            int factorX = depth.xres / textureSize.Width;
            int factorY = ((depth.yres / textureSize.Height) - 1) * depth.xres;

            // invert Y axis while doing the update
            for (int y = textureSize.Height - 1; y >= 0; --y, depthIndex += factorY)
            {
                int outputIndex = y * textureSize.Width;
                for (int x = 0; x < textureSize.Width; ++x, depthIndex += factorX, ++outputIndex)
                {
                    outputPixels[outputIndex] = depthToColor[rawDepthMap[depthIndex]];
                }
            }

            if (userSkeleton != null && showBones)
            {
                DrawBones(userSkeleton);
            }
            if (userSkeleton != null && showNodes)
            {
                DrawJoints(userSkeleton);
            }

            texture.SetPixels32(outputPixels);
            texture.Apply();
        }

        /// <summary>
        /// Update the histogram given the depth map.
        /// </summary>
        /// <param name="depth">The depth map</param>
        private void UpdateHistogram(ZigDepth depth)
        {
            int i, numOfPoints = 0;

            BaseSystem.Array.Clear(depthHistogramMap, 0, depthHistogramMap.Length);
            short[] rawDepthMap = depth.data;

            int depthIndex = 0;

            int factorX = depth.xres / textureSize.Width;
            int factorY = ((depth.yres / textureSize.Height) - 1) * depth.xres;
            for (int y = 0; y < textureSize.Height; ++y, depthIndex += factorY)
            {
                for (int x = 0; x < textureSize.Width; ++x, depthIndex += factorX)
                {
                    short pixel = rawDepthMap[depthIndex];
                    if (pixel != 0)
                    {
                        depthHistogramMap[pixel]++;
                        numOfPoints++;
                    }
                }
            }
            depthHistogramMap[0] = 0;
            if (numOfPoints > 0)
            {
                for (i = 1; i < depthHistogramMap.Length; i++)
                {
                    depthHistogramMap[i] += depthHistogramMap[i - 1];
                }
                depthToColor[0] = Color.black;
                for (i = 1; i < depthHistogramMap.Length; i++)
                {
                    float intensity = (1.0f - (depthHistogramMap[i] / numOfPoints));

                    depthToColor[i].r = (byte)(BaseColor.r * intensity);
                    depthToColor[i].g = (byte)(BaseColor.g * intensity);
                    depthToColor[i].b = (byte)(BaseColor.b * intensity);
                    depthToColor[i].a = 255;
                }
            }
        }

        /// <summary>
        /// Superimpose coloured boxes over the joints in the viewer.
        /// </summary>
        /// <param name="userSkeleton">The </param>
        private void DrawJoints(ZigInputJoint[] userSkeleton)
        {
            int scaleZ = 300;
            int xMid = textureSize.Width / 2;
            int yMid = textureSize.Height / 2;
            for (int t = 0; t < userSkeleton.Length; t++)
            {
                if (userSkeleton[t].GoodPosition)
                {
                    int x = xMid - (int)(userSkeleton[t].Position.x * scaleZ / userSkeleton[t].Position.z);
                    int y = yMid - (int)(userSkeleton[t].Position.y * scaleZ / userSkeleton[t].Position.z);

                    Color color = (!userSkeleton[t].Inferred) ? Color.cyan : Color.red;
                    CinemaMocapHelper.drawFastBox(outputPixels, textureSize.Width, textureSize.Height, x - 2, y - 2, x + 2, y + 2, color);
                }
            }
        }

        /// <summary>
        /// Superimpose coloured lines between the joints in the viewer.
        /// </summary>
        /// <param name="userSkeleton">The </param>
        private void DrawBones(ZigInputJoint[] userSkeleton)
        {
            for (int t = 0; t < userSkeleton.Length; t++)
            {
                ZigJointId parentId = CinemaMocapHelper.ParentBoneJoint(userSkeleton[t].Id);
                ZigInputJoint parentJoint = null;

                for (int s = 0; s < userSkeleton.Length; s++)
                {
                    if (userSkeleton[s].Id == parentId)// find parent and leave loop.
                    {
                        parentJoint = userSkeleton[s];
                        break;
                    }
                }

                if (parentJoint != null && parentJoint.Id != ZigJointId.None && userSkeleton[t].GoodPosition && parentJoint.GoodPosition)
                {
                    // parent and child joint coordinates
                    int childX = textureSize.Width / 2 - (int)(userSkeleton[t].Position.x * 300 / userSkeleton[t].Position.z);
                    int childY = textureSize.Height / 2 - (int)(userSkeleton[t].Position.y * 300 / userSkeleton[t].Position.z);
                    int parentX = textureSize.Width / 2 - (int)(parentJoint.Position.x * 300 / parentJoint.Position.z);
                    int parentY = textureSize.Height / 2 - (int)(parentJoint.Position.y * 300 / parentJoint.Position.z);

                    // draw lines between joints
                    Color color = (!userSkeleton[t].Inferred) ? Color.cyan : Color.red;
                    CinemaMocapHelper.drawFastLine(outputPixels, textureSize.Width, textureSize.Height, childX, childY, parentX, parentY, color);
                }
            }
        }
    }
}