
using CinemaSuite.CinemaFaceCap.App.Behaviours;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline
{
    /// <summary>
    /// A Profile for loading rig data and saving animations.
    /// </summary>
    public abstract class OutputProfile
    {
        protected FaceStructure faceStructure;

        /// <summary>
        /// The reference to the Output object, to be loaded as a prefab.
        /// </summary>
        protected GameObject cinema_facecap_prefab;

        /// <summary>
        /// Reference to the output object as a GameObject in the current scene. Used for previewing the item.
        /// </summary>
        protected GameObject cinema_facecap_instance;

        // Recording Save Directory
        protected const string NAME_DUPLICATE_ERROR_MSG = "{0}.anim exists. Saving as {1}.anim...";
        protected const string SAVE_DESTINATION_FORMAT = "{0}/{1}.anim"; // relative path and filename
        protected string filePath = "Assets";
        protected string fileName = "Animation";

        private Texture2D folderIcon;

        /// <summary>
        /// Initialize the OutputProfile and load rig data and prefabs.
        /// </summary>
        public virtual void Initialize()
        {
            faceStructure = GetTargetStructure();

            cinema_facecap_prefab = Resources.Load(faceStructure.AssetName) as GameObject;
            if (cinema_facecap_prefab == null)
            {
                UnityEngine.Debug.LogError(string.Format("{0} is missing from the Resources folder. This item is required for the system.", faceStructure.AssetName));
            }

            // Try to find if the preview item exists in the scene already and assign the reference.
            var previewFaces = GameObject.FindObjectsOfType<FaceShaper>();
            foreach (var fs in previewFaces)
            {
                if (fs.gameObject.name == faceStructure.AssetName)
                {
                    cinema_facecap_instance = fs.gameObject;
                }
            }

            if (EditorPrefs.HasKey(CinemaFaceCapSettingsWindow.AnimationFolderKey))
            {
                filePath = EditorPrefs.GetString(CinemaFaceCapSettingsWindow.AnimationFolderKey);
            }
            else
            {
                filePath = CinemaFaceCapSettingsWindow.AnimationFolderDefaultPath;
                EditorPrefs.SetString(CinemaFaceCapSettingsWindow.AnimationFolderKey, filePath);
            }

            folderIcon = EditorGUIUtility.Load("Cinema Suite/Cinema Face Cap/cs_Folder.png") as Texture2D;
        }

        public abstract FaceStructure GetTargetStructure();

        /// <summary>
        /// Save an animation based on the mocap data.
        /// </summary>
        /// <param name="animation">The animation captured.</param>
        public virtual void SaveAnimation(MappedFaceCapAnimation animation)
        {
            // Check if there is capture data
            if (animation == null)
            {
                return;
            }

            faceStructure = GetTargetStructure();

            var clip = new AnimationClip();
            AnimationCurve[] rotationCurve = new AnimationCurve[4];
            rotationCurve[0] = new AnimationCurve();
            rotationCurve[1] = new AnimationCurve();
            rotationCurve[2] = new AnimationCurve();
            rotationCurve[3] = new AnimationCurve();

            AnimationCurve[] blendShapeCurves = new AnimationCurve[faceStructure.BlendShapeInfo.Count];
            for(int i = 0; i < blendShapeCurves.Length; i++)
            {
                blendShapeCurves[i] = new AnimationCurve();
            }

            // Collect the data
            foreach (var keyframe in animation.Keyframes)
            {
                float time = keyframe.ElapsedTime;
                time = time / 1000f; // convert milliseconds to seconds.

                Quaternion rotation = keyframe.Frame.rotation;

                rotationCurve[0].AddKey(time, rotation.x);
                rotationCurve[1].AddKey(time, rotation.y);
                rotationCurve[2].AddKey(time, rotation.z);
                rotationCurve[3].AddKey(time, rotation.w);

                int index = 0;
                foreach (var blendShape in faceStructure.BlendShapeInfo)
                {
                    if (blendShape.Value.include)
                    {
                        float value = keyframe.Frame.AnimationUnits[blendShape.Value.hint];
                        value = blendShape.Value.mappingFunction(value);

                        blendShapeCurves[index].AddKey(time, value);
                    }
                    index++;
                }
            }

            string relativePath = faceStructure.OrientationNodePath;
            relativePath = relativePath.Replace('.', '/');

            // Set rotation curves
            clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotationCurve[0]);
            clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotationCurve[1]);
            clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotationCurve[2]);
            clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotationCurve[3]);

            int blendShapeIndex = 0;
            foreach (var blendShape in faceStructure.BlendShapeInfo)
            {
                clip.SetCurve(faceStructure.FacePath.Replace('.', '/'), typeof(SkinnedMeshRenderer), string.Format("blendShape.{0}", blendShape.Key), blendShapeCurves[blendShapeIndex++]);
            }

            clip.EnsureQuaternionContinuity();

            if (!AssetDatabase.IsValidFolder(filePath))
            {
                Debug.Log(string.Format("Folder path \"{0}\" did not exist. We made it for you.", filePath));
                var split = filePath.Split('/');
                string buildingPath = "";

                foreach (var folder in split)
                {
                    buildingPath += folder;
                    if (!AssetDatabase.IsValidFolder(buildingPath))
                    {
                        if (!string.IsNullOrEmpty(folder))
                        {
                            AssetDatabase.CreateFolder(buildingPath, folder);
                        }
                    }
                    buildingPath += "/";
                }
            }

            string newFileName = fileName;
            if (System.IO.File.Exists(string.Format(SAVE_DESTINATION_FORMAT, filePath, newFileName)))
            {
                newFileName = Utility.Helper.GetNewFilename(filePath, fileName, "anim");
                UnityEngine.Debug.LogWarning(string.Format(NAME_DUPLICATE_ERROR_MSG, fileName, newFileName));
            }
            AssetDatabase.CreateAsset(clip, string.Format(SAVE_DESTINATION_FORMAT, filePath, newFileName));
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Create a preview model in the Scene View.
        /// </summary>
        public virtual void CreatePreview()
        {
            if (cinema_facecap_instance == null)
            {
                cinema_facecap_instance = PrefabUtility.InstantiatePrefab(cinema_facecap_prefab) as GameObject;
                var shaper = cinema_facecap_instance.AddComponent<FaceShaper>();

                shaper.SetOrientationNodePath(faceStructure.OrientationNodePath);
                shaper.SetSkinnedMeshRendererPath(faceStructure.FacePath);
            }
        }

        /// <summary>
        /// Update the Preview Model.
        /// </summary>
        /// <param name="face">The skeleton to update the preview with.</param>
        public virtual void UpdatePreview(MappedFaceCapFrame face)
        {
            if (cinema_facecap_instance != null)
            {
                var shaper = cinema_facecap_instance.GetComponent<FaceShaper>();

                var weights = new List<FaceShaper.BlendShapeWeight>();

                foreach(var blendShape in faceStructure.BlendShapeInfo)
                {
                    float value = face.AnimationUnits[blendShape.Value.hint];

                    // Map the value
                    value = blendShape.Value.mappingFunction(value);

                    // Use the key to determine blendshape index

                    weights.Add(new FaceShaper.BlendShapeWeight() { key = blendShape.Key, value = value });
                }

                shaper.SetValues(face.rotation, weights);
            }
        }

        /// <summary>
        /// Reset the preview model back to it's initial pose.
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// Destroy the output profile.
        /// </summary>
        public virtual void Destroy()
        {
            GameObject.DestroyImmediate(cinema_facecap_instance);
        }

        /// <summary>
        /// Draw any custom output settings to the UI.
        /// </summary>
        public virtual void DrawOutputSettings()
        {
            fileName = EditorGUILayout.TextField(new GUIContent("Animation Filename"), fileName);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(new GUIContent("Folder", "The folder the newly created file will be placed."), new GUIContent(filePath), EditorStyles.textField, GUILayout.Height(16f));

            bool saveDestChanged = false;
            if (GUILayout.Button(folderIcon, EditorStyles.miniButton, GUILayout.Width(40f), GUILayout.Height(16f)))
            {
                GUI.SetNextControlName("");
                GUI.FocusControl("");

                string temp = EditorUtility.SaveFolderPanel("Select a folder within your project", filePath, "");

                if (!string.IsNullOrEmpty(temp))
                {
                    if (!temp.StartsWith(Application.dataPath))
                    {
                        filePath = "Assets";
                    }
                    else
                    {
                        filePath = "Assets" + temp.Replace(Application.dataPath, string.Empty);
                    }
                }
                saveDestChanged = true;
            }
            EditorGUILayout.EndHorizontal();

            if (saveDestChanged)
            {
                EditorPrefs.SetString(CinemaFaceCapSettingsWindow.AnimationFolderKey, filePath);
            }
        }

        /// <summary>
        /// Load all output profiles context data found in the assembly.
        /// </summary>
        public static List<TypeLabelContextData> LoadMetaData(List<Type> outputTypes)
        {
            var outputProfiles = new List<TypeLabelContextData>();

            List<Type> types = Utility.Helper.GetOutputProfiles();
            foreach (Type t in types)
            {
                bool isChild = false;
                foreach(var parent in outputTypes)
                {
                    if(t.IsSubclassOf(parent))
                    {
                        isChild = true;
                        break;
                    }
                }
                if (!t.IsAbstract && isChild)
                {
                    foreach (NameAttribute attribute in t.GetCustomAttributes(typeof(NameAttribute), true))
                    {
                        outputProfiles.Add(new TypeLabelContextData(t, attribute.Name));
                    }
                }
            }

            return outputProfiles;
        }
    }
}
