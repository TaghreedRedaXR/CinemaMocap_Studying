using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.Viewers
{
    [SensorEditorViewer("Image Viewer", 1, SupportedDevice.Kinect1)]
    public class Kinect1EditorImageViewer : Kinect1EditorViewer
    {
        // GUI
        private Texture nodesImage = null;
        private Texture bonesImage = null;

        // Options
        private bool showNodes = true;
        private bool showBones = true;
        private ZigResolution TextureSize = ZigResolution.QVGA_320_x_240;


        internal Texture2D texture;
        internal ResolutionData textureSize;
        Color32[] outputPixels;

        /// <summary>
        /// Initialize the Kinect1 Image Viewer
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

            Rect resolutionArea = button1;
            resolutionArea.x -= 120;
            resolutionArea.width = 120;

            //List<GUIContent> resolutions = new List<GUIContent>();
            //foreach (string name in Enum.GetNames(typeof(ZigResolution)))
            //{
            //    resolutions.Add(new GUIContent(name.Replace('_', ' ')));
            //}
            //int result = EditorGUI.Popup(resolutionArea, new GUIContent("", "Image Resolution"), (int)TextureSize, resolutions.ToArray(), EditorStyles.toolbarDropDown);
            //if(result != (int)TextureSize)
            //{
            //    TextureSize = (ZigResolution)result;
            //    Initialize();
            //}

            showNodes = GUI.Toggle(button1, showNodes, new GUIContent("", nodesImage, "Show Nodes"), EditorStyles.toolbarButton);
            showBones = GUI.Toggle(button2, showBones, new GUIContent("", bonesImage, "Show Bones"), EditorStyles.toolbarButton);
        }

        public override void Update(ZigEditorInput instance, int mainUserId)
        {
            if (instance == null || ZigEditorInput.Image == null) return;
            if (texture == null)
            {
                Initialize();
            }

            // Get the user Skeleton
            ZigInputJoint[] skeleton = null;
            if (instance.TrackedUsers.ContainsKey(mainUserId))
            {
                skeleton = instance.TrackedUsers[mainUserId].Skeleton;
            }

            // Update the texture
            UpdateTexture(ZigEditorInput.Image, skeleton);
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

        private void UpdateTexture(ZigImage image, ZigInputJoint[] userSkeleton)
        {
            if (image.data == null) return;

            Color32[] rawImageMap = image.data;
            int srcIndex = 0;
            int factorX = image.xres / textureSize.Width;
            int factorY = ((image.yres / textureSize.Height) - 1) * image.xres;

            // invert Y axis while doing the update
            for (int y = textureSize.Height - 1; y >= 0; --y, srcIndex += factorY)
            {
                int outputIndex = y * textureSize.Width;
                for (int x = 0; x < textureSize.Width; ++x, srcIndex += factorX, ++outputIndex)
                {
                    outputPixels[outputIndex] = rawImageMap[srcIndex];
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

        private void DrawJoints(ZigInputJoint[] userSkeleton)
        {
            int xMid = textureSize.Width / 2;
            int yMid = textureSize.Height / 2;
            for (int t = 0; t < userSkeleton.Length; t++)
            {
                if (userSkeleton[t].GoodPosition)
                {
                    int x = xMid - (int)(userSkeleton[t].Position.x * 300 / userSkeleton[t].Position.z);
                    int y = yMid - (int)(userSkeleton[t].Position.y * 300 / userSkeleton[t].Position.z);

                    Color color = (!userSkeleton[t].Inferred) ? Color.cyan : Color.red;
                    CinemaMocapHelper.drawFastBox(outputPixels, textureSize.Width, textureSize.Height, x - 2, y - 2, x + 2, y + 2, color);
                }
            }
        }

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