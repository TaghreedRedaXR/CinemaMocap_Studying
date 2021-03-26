
using CinemaSuite.CinemaMocap.System.Core.Collada;
using CinemaSuite.CinemaMocap.System.Core;
using BaseSystem = System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CinemaSuite.CinemaMocap.System.Behaviours;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline.Output
{
    [OutputProfileAttribute("25 Joint dae")]
    public class Standard25Joint : OutputProfile
    {
        // Folders
        private const string SKELETON_NAME = "Cinema_Mocap_Humanoid_25_Joint";
        private const string SOURCE_FILE = "Cinema_Mocap_Humanoid_25_Joint.dae";
        private const string SOURCE_FILE_PATH = "Assets/Cinema Suite/Cinema Mocap/Resources/Cinema_Mocap_Humanoid_25_Joint.dae";
        private const string SOURCE_FILE_MATRIX_PATH = "Assets/Cinema Suite/Cinema Mocap/Resources/Cinema_Mocap_Humanoid_25_Joint_Matrix.dae";
        private const string SOURCE_FILE_PATH_60FPS = "Assets/Cinema Suite/Cinema Mocap/Resources/Cinema_Mocap_Humanoid_25_Joint_60_FPS.dae";
        private const string SOURCE_FILE_MATRIX_PATH_60FPS = "Assets/Cinema Suite/Cinema Mocap/Resources/Cinema_Mocap_Humanoid_25_Joint_Matrix_60_FPS.dae";
        private const string NAME_DUPLICATE_ERROR_MSG = "{0}.dae exists. Saving as {1}.dae...";
        private const string SAVE_DESTINATION_FORMAT = "{0}/{1}.dae"; // relative path and filename

        // EditorPrefs
        private const string TRANSFORMATION_TYPE_KEY = "CinemaSuite.Output25JointTransformationType";
        private const string FRAME_RATE_INDEX_KEY = "CinemaSuite.Output25JointFrameRate";
        private const string FILENAME_KEY = "CinemaSuite.Output25JointFilename";
        private const string SAVE_DESTINATION_KEY = "CinemaSuite.Output25JointSaveDestination";

        private string animSaveDestination = "Assets";
        private string fileName = "Animation";
        private string absPath = Application.dataPath; // Path returned by the folder selection panel

        private ColladaRigData rigData;

        private GameObject cinema_mocap_humanoid_prefab;
        private GameObject cinema_mocap_humanoid_instance;

        // Recording options
        private TransformationType transformationType = TransformationType.TransRotLoc;
        private int selectedFrameRateIndex = 0;

        // Folders
        private const string FILE_DESTINATION = "Assets/Cinema Suite/Cinema Mocap/Animations//{0}.dae";

        /// <summary>
        /// COLLADA encoding types (Advanced)
        /// </summary>
        private enum TransformationType
        {
            TransRotLoc,
            Matrix
        }

        private static NUIJointType ColladaToNUIJointMapping(string colladaJointId)
        {
            switch (colladaJointId)
            {
                case "SpineBase":
                    return NUIJointType.SpineBase;
                case "SpineMid":
                    return NUIJointType.SpineMid;
                case "SpineShoulder":
                    return NUIJointType.SpineShoulder;
                case "Neck":
                    return NUIJointType.Neck;
                case "Head":
                    return NUIJointType.Head;
                case "HipRight":
                    return NUIJointType.HipRight;
                case "KneeRight":
                    return NUIJointType.KneeRight;
                case "AnkleRight":
                    return NUIJointType.AnkleRight;
                case "FootRight":
                    return NUIJointType.FootRight;
                case "HipLeft":
                    return NUIJointType.HipLeft;
                case "KneeLeft":
                    return NUIJointType.KneeLeft;
                case "AnkleLeft":
                    return NUIJointType.AnkleLeft;
                case "FootLeft":
                    return NUIJointType.FootLeft;
                case "ShoulderRight":
                    return NUIJointType.ShoulderRight;
                case "ElbowRight":
                    return NUIJointType.ElbowRight;
                case "WristRight":
                    return NUIJointType.WristRight;
                case "HandRight":
                    return NUIJointType.HandRight;
                case "HandTipRight":
                    return NUIJointType.HandTipRight;
                case "ThumbRight":
                    return NUIJointType.ThumbRight;
                case "ShoulderLeft":
                    return NUIJointType.ShoulderLeft;
                case "ElbowLeft":
                    return NUIJointType.ElbowLeft;
                case "WristLeft":
                    return NUIJointType.WristLeft;
                case "HandLeft":
                    return NUIJointType.HandLeft;
                case "HandTipLeft":
                    return NUIJointType.HandTipLeft;
                case "ThumbLeft":
                    return NUIJointType.ThumbLeft;
                default:
                    return NUIJointType.Unspecified;
            }
        }

        public static string NUIJointToColladaMapping(NUIJointType nuiJointId)
        {
            return BaseSystem.Enum.GetName(typeof(NUIJointType), nuiJointId);
        }

        public override void Initialize()
        {
            rigData = ColladaUtility.ReadRigData(SOURCE_FILE_PATH);

            cinema_mocap_humanoid_prefab = Resources.Load(SKELETON_NAME) as GameObject;
            if (cinema_mocap_humanoid_prefab == null)
            {
                UnityEngine.Debug.LogError(string.Format("{0} is missing from the Resources folder. This item is required for the system.", SOURCE_FILE));
            }

            // Try to find if the Skeleton exists in the scene already and assign the reference.
            HumanoidPoser[] previewSkeletons = GameObject.FindObjectsOfType<HumanoidPoser>();
            foreach (HumanoidPoser rhp in previewSkeletons)
            {
                if (rhp.gameObject.name == SKELETON_NAME)
                {
                    cinema_mocap_humanoid_instance = rhp.gameObject;
                }
            }
            
            LoadEditorPrefs();
        }

        /// <summary>
        /// Get the structure for what is expected in this output method.
        /// </summary>
        /// <returns></returns>
        public override NUISkeleton GetTargetStructure()
        {
            // Define the skeletal structure
            SkeletonStructure structure = new SkeletonStructure();

            structure.SetRootJoint(NUIJointType.SpineBase);
            structure.AddBone(ColladaToNUIJointMapping("SpineMid"), ColladaToNUIJointMapping("SpineBase"));
            structure.AddBone(ColladaToNUIJointMapping("SpineShoulder"), ColladaToNUIJointMapping("SpineMid"));
            structure.AddBone(ColladaToNUIJointMapping("Neck"), ColladaToNUIJointMapping("SpineShoulder"));
            structure.AddBone(ColladaToNUIJointMapping("Head"), ColladaToNUIJointMapping("Neck"));

            structure.AddBone(ColladaToNUIJointMapping("ShoulderLeft"), ColladaToNUIJointMapping("SpineShoulder"));
            structure.AddBone(ColladaToNUIJointMapping("ElbowLeft"), ColladaToNUIJointMapping("ShoulderLeft"));
            structure.AddBone(ColladaToNUIJointMapping("WristLeft"), ColladaToNUIJointMapping("ElbowLeft"));
            structure.AddBone(ColladaToNUIJointMapping("HandLeft"), ColladaToNUIJointMapping("WristLeft"));
            structure.AddBone(ColladaToNUIJointMapping("HandTipLeft"), ColladaToNUIJointMapping("HandLeft"));
            structure.AddBone(ColladaToNUIJointMapping("ThumbLeft"), ColladaToNUIJointMapping("HandLeft"));

            structure.AddBone(ColladaToNUIJointMapping("ShoulderRight"), ColladaToNUIJointMapping("SpineShoulder"));
            structure.AddBone(ColladaToNUIJointMapping("ElbowRight"), ColladaToNUIJointMapping("ShoulderRight"));
            structure.AddBone(ColladaToNUIJointMapping("WristRight"), ColladaToNUIJointMapping("ElbowRight"));
            structure.AddBone(ColladaToNUIJointMapping("HandRight"), ColladaToNUIJointMapping("WristRight"));
            structure.AddBone(ColladaToNUIJointMapping("HandTipRight"), ColladaToNUIJointMapping("HandRight"));
            structure.AddBone(ColladaToNUIJointMapping("ThumbRight"), ColladaToNUIJointMapping("HandRight"));

            structure.AddBone(ColladaToNUIJointMapping("HipLeft"), ColladaToNUIJointMapping("SpineBase"));
            structure.AddBone(ColladaToNUIJointMapping("KneeLeft"), ColladaToNUIJointMapping("HipLeft"));
            structure.AddBone(ColladaToNUIJointMapping("AnkleLeft"), ColladaToNUIJointMapping("KneeLeft"));
            structure.AddBone(ColladaToNUIJointMapping("FootLeft"), ColladaToNUIJointMapping("AnkleLeft"));

            structure.AddBone(ColladaToNUIJointMapping("HipRight"), ColladaToNUIJointMapping("SpineBase"));
            structure.AddBone(ColladaToNUIJointMapping("KneeRight"), ColladaToNUIJointMapping("HipRight"));
            structure.AddBone(ColladaToNUIJointMapping("AnkleRight"), ColladaToNUIJointMapping("KneeRight"));
            structure.AddBone(ColladaToNUIJointMapping("FootRight"), ColladaToNUIJointMapping("AnkleRight"));

            // Define the skeleton in Unity terms.
            NUISkeleton skeleton = new NUISkeleton(structure);

            // Get the hip right and the chest right.
            Vector3 hipRightTranslation = rigData.GetJoint("HipRight").LHSWorldTransformationMatrix.GetColumn(3);
            Vector3 hipLeftTranslation = rigData.GetJoint("HipLeft").LHSWorldTransformationMatrix.GetColumn(3);
            Vector3 shoulderRightTranslation = rigData.GetJoint("ShoulderRight").LHSWorldTransformationMatrix.GetColumn(3);
            Vector3 shoulderLeftTranslation = rigData.GetJoint("ShoulderLeft").LHSWorldTransformationMatrix.GetColumn(3);
            
            Vector3 hipRight = hipRightTranslation - hipLeftTranslation;
            Vector3 chestRight = shoulderRightTranslation - shoulderLeftTranslation;
            skeleton.SpineBaseRight = rigData.GetJoint("SpineBase").LHSWorldTransformationMatrix.inverse.MultiplyVector(hipRight);
            skeleton.ChestRight = rigData.GetJoint("SpineMid").LHSWorldTransformationMatrix.inverse.MultiplyVector(chestRight);

            foreach (KeyValuePair<string, ColladaJointData> jointData in rigData.JointData)
            {
                NUIJointType jointType = ColladaToNUIJointMapping(jointData.Key);
                NUIJoint joint = new NUIJoint(jointType);

                // Convert the Collada Joint rotation from RHS to LHS (Unity)
                ColladaJointData currentJoint = jointData.Value;
                joint.Position = currentJoint.Translation;
                joint.Rotation = QuaternionHelper.RHStoLHS(currentJoint.RotationVector);
                joint.TransformationMatrix = currentJoint.LHSTransformationMatrix;
                joint.WorldTransformationMatrix = currentJoint.LHSWorldTransformationMatrix;

                Vector3 directionToChild = Vector3.zero;

                if (!structure.IsJointAnExtremity(jointType)) // directionToChild is not needed for extremeties.
                {
                    NUIJointType childType = structure.GetChildJoint(jointType);
                    if (childType != NUIJointType.Unspecified)
                    {
                        Vector3 child = rigData.GetJoint(NUIJointToColladaMapping(childType)).LHSWorldTransformationMatrix.GetColumn(3);
                        Vector3 parent = currentJoint.LHSWorldTransformationMatrix.GetColumn(3);
                        directionToChild = child - parent;
                    }
                }

                if (jointType == NUIJointType.SpineBase) // The Hip is a special case.
                {
                    Vector3 rightHipWorldPosition = rigData.GetJoint("HipRight").LHSWorldTransformationMatrix.GetColumn(3);
                    Vector3 leftHipWorldPosition = rigData.GetJoint("HipLeft").LHSWorldTransformationMatrix.GetColumn(3);
                    Vector3 hipWorldPosition = rigData.GetJoint("SpineBase").LHSWorldTransformationMatrix.GetColumn(3);

                    directionToChild = ((rightHipWorldPosition + leftHipWorldPosition) / 2F) - hipWorldPosition;

                    //float angle = Vector3.Angle(Vector3.up, directionToChild);
                    //directionToChild = Quaternion.AngleAxis(40 - angle, hipRight) * directionToChild;
                }

                joint.directionToChild = currentJoint.LHSWorldTransformationMatrix.inverse.MultiplyVector(directionToChild);
                
                skeleton.Joints.Add(jointType, joint);
            }
            skeleton.ChestRight -= Vector3.Project(skeleton.ChestRight, skeleton.Joints[NUIJointType.SpineMid].directionToChild);

            return skeleton;
        }

        public override void DrawOutputSettings()
        {
            // Check if save location has changed from settings window
            if (animSaveDestination != EditorPrefs.GetString(SAVE_DESTINATION_KEY))
            {
                animSaveDestination = EditorPrefs.GetString(SAVE_DESTINATION_KEY);
                absPath = (Application.dataPath).Replace("Assets", animSaveDestination);
            }

            TransformationType tempTransformationType = (TransformationType)EditorGUILayout.EnumPopup(new GUIContent("Transformation Type"), transformationType);

            if (tempTransformationType != transformationType)
            {
                transformationType = tempTransformationType;
                EditorPrefs.SetInt(TRANSFORMATION_TYPE_KEY, (int)transformationType);
            }

            //int tempFrameRate = EditorGUILayout.IntField(new GUIContent("Frame Rate", "The amount of frames to create per second in the output animation."), frameRate);

            //if (tempFrameRate != frameRate)
            //{
            //    frameRate = tempFrameRate;
            //    EditorPrefs.SetInt(FRAME_RATE_KEY, frameRate);
            //}



            int tempFrameRateIndex = EditorGUILayout.Popup("Frame Rate", selectedFrameRateIndex, new string[2] { "30 FPS", "60 FPS" });

            if (tempFrameRateIndex != selectedFrameRateIndex)
            {
                selectedFrameRateIndex = tempFrameRateIndex;
                EditorPrefs.SetInt(FRAME_RATE_INDEX_KEY, selectedFrameRateIndex);
            }

            string tempFileName = EditorGUILayout.TextField(new GUIContent("Animation Name", "The name of the animation when saved to .dae format."), fileName);

            if (tempFileName != fileName)
            {
                fileName = tempFileName;
                EditorPrefs.SetString(FILENAME_KEY, fileName);
            }

            // Save Path Field & Button

            EditorGUILayout.PrefixLabel("Save Location:");

            EditorGUILayout.BeginHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.SelectableLabel(animSaveDestination, EditorStyles.textField, GUILayout.Height(16f));
            EditorGUI.indentLevel--;

            bool saveDestChanged = false;
            if (GUILayout.Button(new GUIContent("..."), EditorStyles.miniButton, GUILayout.Width(24f)))
            {
                // Ensure the path text field does not have focus, or else we cannot change the contents.
                GUI.SetNextControlName("");
                GUI.FocusControl("");

                string temp = EditorUtility.SaveFolderPanel("Select a folder within your project", animSaveDestination, "");

                // Pressing cancel returns an empty string, don't clear the previous text on cancel.
                if (temp != string.Empty) absPath = temp;

                saveDestChanged = true;
            }

            EditorGUILayout.EndHorizontal();

            // Display error if path not within project folder
            if (!absPath.StartsWith(Application.dataPath))
            {
                EditorGUILayout.HelpBox("Invalid selection!\nYou must select a location within your project's \"Assets\" folder.", MessageType.Warning);
                animSaveDestination = CinemaMocapHelper.GetRelativeProjectPath(Application.dataPath);
            }
            else
            {
                animSaveDestination = CinemaMocapHelper.GetRelativeProjectPath(absPath);
            }

            if (saveDestChanged)
            {
                EditorPrefs.SetString(SAVE_DESTINATION_KEY, animSaveDestination);
            }
        }

        public override void Destroy()
        {
            if (cinema_mocap_humanoid_instance != null)
            {
                GameObject.DestroyImmediate(cinema_mocap_humanoid_instance);
            }
        }

        public override void CreatePreview()
        {
            if (cinema_mocap_humanoid_instance == null)
            {
                cinema_mocap_humanoid_instance = PrefabUtility.InstantiatePrefab(cinema_mocap_humanoid_prefab) as GameObject;
                var posing = cinema_mocap_humanoid_instance.AddComponent<HumanoidPoser>();
                posing.Initialize(rigData.GetJointNames());
            }
        }

        public override void Reset()
        {
            NUISkeleton skeleton = GetTargetStructure();
            UpdatePreview(skeleton, Vector3.zero);
        }

        public override void SaveEditorPrefs()
        {
            EditorPrefs.SetInt(TRANSFORMATION_TYPE_KEY, (int)transformationType);
            EditorPrefs.SetInt(FRAME_RATE_INDEX_KEY, selectedFrameRateIndex);
            EditorPrefs.SetString(FILENAME_KEY, fileName);
            EditorPrefs.SetString(SAVE_DESTINATION_KEY, animSaveDestination);
        }

        public override void LoadEditorPrefs()
        {
            if (EditorPrefs.HasKey(TRANSFORMATION_TYPE_KEY))
            {
                transformationType = (TransformationType)EditorPrefs.GetInt(TRANSFORMATION_TYPE_KEY, 0);
            }
            if (EditorPrefs.HasKey(FRAME_RATE_INDEX_KEY))
            {
                selectedFrameRateIndex = EditorPrefs.GetInt(FRAME_RATE_INDEX_KEY, 0);
            }
            if (EditorPrefs.HasKey(FILENAME_KEY))
            {
                fileName = EditorPrefs.GetString(FILENAME_KEY, "Animation");
            }
            if (EditorPrefs.HasKey(SAVE_DESTINATION_KEY))
            {
                animSaveDestination = EditorPrefs.GetString(SAVE_DESTINATION_KEY, "Assets");
                absPath = (Application.dataPath).Replace("Assets", animSaveDestination);
            }
        }

        public override void UpdatePreview(NUISkeleton skeleton, Vector3 position)
        {
            if (cinema_mocap_humanoid_instance != null)
            {
                HumanoidPoser poser = cinema_mocap_humanoid_instance.GetComponent<HumanoidPoser>();
                var rotations = new Dictionary<string, Quaternion>();

                foreach (var pair in skeleton.Joints)
                {
                    rotations.Add(NUIJointToColladaMapping(pair.Key), pair.Value.Rotation);
                }

                poser.SetWorldPosition(position);
                poser.SetRotations(rotations);
            }
        }

        public override void SaveAnimation(NUIHumanoidAnimation animation)
        {
            // Check if there is capture data
            if (animation == null)
            {
                UnityEngine.Debug.LogWarning("No capture data was found.");
                return;
            }

            // Map captured data to Collada data
            ColladaAnimationData data = GetColladaAnimation(animation);

            // Check filename
            string appendedFileName = string.Format("MoCapHumanoid@{0}", fileName);
            string newFileName = appendedFileName;
            if (BaseSystem.IO.File.Exists(string.Format(SAVE_DESTINATION_FORMAT, animSaveDestination, newFileName)))
            {
                newFileName = CinemaMocapHelper.GetNewFilename(animSaveDestination, appendedFileName, "dae");
                UnityEngine.Debug.LogWarning(string.Format(NAME_DUPLICATE_ERROR_MSG, appendedFileName, newFileName));
            }

            // Save
            if (transformationType == TransformationType.Matrix)
            {
                ColladaUtility.SaveAnimationData(data, SOURCE_FILE_MATRIX_PATH, string.Format(SAVE_DESTINATION_FORMAT, animSaveDestination, newFileName), true);
            }
            else
            {
                ColladaUtility.SaveAnimationData(data, SOURCE_FILE_PATH, string.Format(SAVE_DESTINATION_FORMAT, animSaveDestination, newFileName), false);
            }
        }

        public ColladaAnimationData GetColladaAnimation(NUIHumanoidAnimation animation)
        {
            NUISkeleton outputStructure = GetTargetStructure();

            ColladaAnimationData colladaAnimation = new ColladaAnimationData(rigData);
            List<float> elapsedTimes = new List<float>();

            Vector3 startingPosition = Vector3.zero;
            Vector3 rigStartingPosition = rigData.GetJoint("SpineBase").Translation;


            int frameRate = 0;
            switch (selectedFrameRateIndex)
            {
                case 0:
                    frameRate = 30;
                    break;
                case 1:
                    frameRate = 60;
                    break;
            }

            NUIHumanoidAnimation animationConstrained = animation.ConstrainFramerate(frameRate);

            for (int k = 0; k < animationConstrained.Keyframes.Count; k++)
            {
                NUIAnimationKeyframe keyframe = animationConstrained.Keyframes[k];

                elapsedTimes.Add(keyframe.ElapsedTime);

                ColladaRigData currentRig = new ColladaRigData();
                ColladaJointData parentJoint = rigData.GetJoint("SpineBase");
                currentRig.Add(parentJoint.Id, parentJoint);

                foreach (KeyValuePair<string, ColladaJointData> jointData in rigData.JointData)
                {
                    ColladaJointData colladaJointData = rigData.GetJoint(jointData.Key);

                    if (colladaJointData.Id == "SpineBase")
                    {
                        Vector3 hipPosition = keyframe.Skeleton.Joints[NUIJointType.SpineBase].Position * 100;
                        hipPosition.x *= -1;
                        if (startingPosition == Vector3.zero)
                        {
                            startingPosition = hipPosition;
                        }
                        colladaJointData.Translation = (hipPosition - startingPosition) + rigStartingPosition;
                    }
                    colladaAnimation.jointTranslateX[jointData.Key] += string.Format("{0} ", colladaJointData.Translation.x);
                    colladaAnimation.jointTranslateY[jointData.Key] += string.Format("{0} ", colladaJointData.Translation.y);
                    colladaAnimation.jointTranslateZ[jointData.Key] += string.Format("{0} ", colladaJointData.Translation.z);
                }

                foreach (KeyValuePair<NUIJointType, NUIJoint> kvp in keyframe.Skeleton.Joints)
                {
                    // For parent joints
                    Quaternion rotation = kvp.Value.Rotation;

                    string id = NUIJointToColladaMapping(kvp.Key);
                    ColladaJointData colladaJointData = rigData.GetJoint(id);

                    if (!outputStructure.Structure.IsJointAnExtremity(kvp.Key))
                    {
                        Vector3 revert = QuaternionHelper.ToEulerAnglesXYZ(rotation);
                        Vector3 corrected = new Vector3(revert.x, -revert.y, -revert.z);

                        colladaAnimation.jointRotateX[id] += string.Format("{0} ", corrected.x);
                        colladaAnimation.jointRotateY[id] += string.Format("{0} ", corrected.y);
                        colladaAnimation.jointRotateZ[id] += string.Format("{0} ", corrected.z);

                        Matrix4x4 transformation = Matrix4x4.TRS(colladaJointData.Translation, QuaternionHelper.FromEulerAnglesXYZ(corrected), Vector3.one);
                        colladaAnimation.jointValues[id] += string.Format("{0} ", transformation.ToString());
                    }
                    else
                    {
                        // Extremeties
                        colladaAnimation.jointRotateX[id] += string.Format("{0} ", colladaJointData.RotationVector.x);
                        colladaAnimation.jointRotateY[id] += string.Format("{0} ", colladaJointData.RotationVector.y);
                        colladaAnimation.jointRotateZ[id] += string.Format("{0} ", colladaJointData.RotationVector.z);

                        Matrix4x4 transformation = Matrix4x4.TRS(colladaJointData.Translation, colladaJointData.Rotation, Vector3.one);
                        colladaAnimation.jointValues[id] += string.Format("{0} ", transformation.ToString());
                    }
                }
            }

            colladaAnimation.frameTimelapse = elapsedTimes;
            return colladaAnimation;
        }
    }
}