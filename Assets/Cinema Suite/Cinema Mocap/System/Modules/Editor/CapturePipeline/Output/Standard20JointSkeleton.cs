
using BaseSystem = System;
using CinemaSuite.CinemaMocap.System.Core.Collada;
using CinemaSuite.CinemaMocap.System.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CinemaSuite.CinemaMocap.System.Behaviours;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline.Output
{
    [OutputProfileAttribute("20 Joint dae")]
    public class Standard20Joint : OutputProfile
    {
        // Folders
        private const string SKELETON_NAME = "Cinema_Mocap_Humanoid";
        private const string SOURCE_FILE = "Assets/Cinema Suite/Cinema Mocap/Resources/Cinema_Mocap_Humanoid.dae";
        private const string SOURCE_FILE_MATRIX = "Assets/Cinema Suite/Cinema Mocap/Resources/Cinema_Mocap_Humanoid_Matrix.dae";
        private const string SOURCE_FILE_60FPS = "Assets/Cinema Suite/Cinema Mocap/Resources/Cinema_Mocap_Humanoid_60_FPS.dae";
        private const string SOURCE_FILE_MATRIX_60FPS = "Assets/Cinema Suite/Cinema Mocap/Resources/Cinema_Mocap_Humanoid_Matrix_60_FPS.dae";
        private const string NAME_DUPLICATE_ERROR_MSG = "{0}.dae exists. Saving as {1}.dae...";
        private const string SAVE_DESTINATION_FORMAT = "{0}/{1}.dae"; // relative path and filename

        // EditorPrefs
        private const string TRANSFORMATION_TYPE_KEY = "CinemaSuite.Output20JointTransformationType";
        private const string FRAME_RATE_INDEX_KEY = "CinemaSuite.Output20JointFrameRate";
        private const string FILENAME_KEY = "CinemaSuite.Output20JointFilename";
        private const string SAVE_DESTINATION_KEY = "CinemaSuite.Output20JointSaveDestination";
        
        private string animSaveDestination = "Assets";
        private string fileName = "Animation";
        private string absPath = Application.dataPath; // Path returned by the folder selection panel

        private ColladaRigData rigData;

        private GameObject cinema_mocap_humanoid_prefab;
        private GameObject cinema_mocap_humanoid_instance;

        // Recording options
        private TransformationType transformationType = TransformationType.TransRotLoc;
        private int selectedFrameRateIndex = 0;

        /// <summary>
        /// COLLADA encoding types (Advanced)
        /// </summary>
        private enum TransformationType
        {
            TransRotLoc,
            Matrix
        }

        public static NUIJointType ColladaToNUIJointMapping(string colladaJointId)
        {
            switch (colladaJointId)
            {
                case "HIP":
                    return NUIJointType.SpineBase;
                case "SPINE":
                    return NUIJointType.SpineMid;
                case "SHOULDER_CENTER":
                    return NUIJointType.Neck;
                case "HEAD":
                    return NUIJointType.Head;
                case "HIP_RIGHT":
                    return NUIJointType.HipRight;
                case "KNEE_RIGHT":
                    return NUIJointType.KneeRight;
                case "ANKLE_RIGHT":
                    return NUIJointType.AnkleRight;
                case "FOOT_RIGHT":
                    return NUIJointType.FootRight;
                case "HIP_LEFT":
                    return NUIJointType.HipLeft;
                case "KNEE_LEFT":
                    return NUIJointType.KneeLeft;
                case "ANKLE_LEFT":
                    return NUIJointType.AnkleLeft;
                case "FOOT_LEFT":
                    return NUIJointType.FootLeft;
                case "SHOULDER_RIGHT":
                    return NUIJointType.ShoulderRight;
                case "ELBOW_RIGHT":
                    return NUIJointType.ElbowRight;
                case "WRIST_RIGHT":
                    return NUIJointType.WristRight;
                case "HAND_RIGHT":
                    return NUIJointType.HandRight;
                case "SHOULDER_LEFT":
                    return NUIJointType.ShoulderLeft;
                case "ELBOW_LEFT":
                    return NUIJointType.ElbowLeft;
                case "WRIST_LEFT":
                    return NUIJointType.WristLeft;
                case "HAND_LEFT":
                    return NUIJointType.HandLeft;
                default:
                    return NUIJointType.Unspecified;
            }
        }

        public static string NUIJointToColladaMapping(NUIJointType nuiJointId)
        {
            switch (nuiJointId)
            {
                case NUIJointType.SpineBase:
                    return "HIP";
                case NUIJointType.SpineMid:
                    return "SPINE";
                case NUIJointType.Neck:
                    return "SHOULDER_CENTER";
                case NUIJointType.Head:
                    return "HEAD";
                case NUIJointType.HipRight:
                    return "HIP_RIGHT";
                case NUIJointType.KneeRight:
                    return "KNEE_RIGHT";
                case NUIJointType.AnkleRight:
                    return "ANKLE_RIGHT";
                case NUIJointType.FootRight:
                    return "FOOT_RIGHT";
                case NUIJointType.HipLeft:
                    return "HIP_LEFT";
                case NUIJointType.KneeLeft:
                    return "KNEE_LEFT";
                case NUIJointType.AnkleLeft:
                    return "ANKLE_LEFT";
                case NUIJointType.FootLeft:
                    return "FOOT_LEFT";
                case NUIJointType.ShoulderRight:
                    return "SHOULDER_RIGHT";
                case NUIJointType.ElbowRight:
                    return "ELBOW_RIGHT";
                case NUIJointType.WristRight:
                    return "WRIST_RIGHT";
                case NUIJointType.HandRight:
                    return "HAND_RIGHT";
                case NUIJointType.ShoulderLeft:
                    return "SHOULDER_LEFT";
                case NUIJointType.ElbowLeft:
                    return "ELBOW_LEFT";
                case NUIJointType.WristLeft:
                    return "WRIST_LEFT";
                case NUIJointType.HandLeft:
                    return "HAND_LEFT";
                default:
                    return "HIP";
            }
        }

        public override void Initialize()
        {
            rigData = ColladaUtility.ReadRigData(SOURCE_FILE);

            cinema_mocap_humanoid_prefab = Resources.Load(SKELETON_NAME) as GameObject;
            if (cinema_mocap_humanoid_prefab == null)
            {
                UnityEngine.Debug.LogError("Cinema_Mocap_Humanoid.dae is missing from the Resources folder. This item is required for the system.");
            }

            // Try to find if the Skeleton exists in the scene already and assign the reference.
            HumanoidPoser[] previewSkeletons = GameObject.FindObjectsOfType<HumanoidPoser>();
            foreach(HumanoidPoser poser in previewSkeletons)
            {
                if(poser.gameObject.name == SKELETON_NAME)
                {
                    cinema_mocap_humanoid_instance = poser.gameObject;
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
            structure.AddBone(ColladaToNUIJointMapping("SPINE"), ColladaToNUIJointMapping("HIP"));
            structure.AddBone(ColladaToNUIJointMapping("SHOULDER_CENTER"), ColladaToNUIJointMapping("SPINE"));
            structure.AddBone(ColladaToNUIJointMapping("HEAD"), ColladaToNUIJointMapping("SHOULDER_CENTER"));

            structure.AddBone(ColladaToNUIJointMapping("SHOULDER_LEFT"), ColladaToNUIJointMapping("SHOULDER_CENTER"));
            structure.AddBone(ColladaToNUIJointMapping("ELBOW_LEFT"), ColladaToNUIJointMapping("SHOULDER_LEFT"));
            structure.AddBone(ColladaToNUIJointMapping("WRIST_LEFT"), ColladaToNUIJointMapping("ELBOW_LEFT"));
            structure.AddBone(ColladaToNUIJointMapping("HAND_LEFT"), ColladaToNUIJointMapping("WRIST_LEFT"));

            structure.AddBone(ColladaToNUIJointMapping("SHOULDER_RIGHT"), ColladaToNUIJointMapping("SHOULDER_CENTER"));
            structure.AddBone(ColladaToNUIJointMapping("ELBOW_RIGHT"), ColladaToNUIJointMapping("SHOULDER_RIGHT"));
            structure.AddBone(ColladaToNUIJointMapping("WRIST_RIGHT"), ColladaToNUIJointMapping("ELBOW_RIGHT"));
            structure.AddBone(ColladaToNUIJointMapping("HAND_RIGHT"), ColladaToNUIJointMapping("WRIST_RIGHT"));

            structure.AddBone(ColladaToNUIJointMapping("HIP_LEFT"), ColladaToNUIJointMapping("HIP"));
            structure.AddBone(ColladaToNUIJointMapping("KNEE_LEFT"), ColladaToNUIJointMapping("HIP_LEFT"));
            structure.AddBone(ColladaToNUIJointMapping("ANKLE_LEFT"), ColladaToNUIJointMapping("KNEE_LEFT"));
            structure.AddBone(ColladaToNUIJointMapping("FOOT_LEFT"), ColladaToNUIJointMapping("ANKLE_LEFT"));

            structure.AddBone(ColladaToNUIJointMapping("HIP_RIGHT"), ColladaToNUIJointMapping("HIP"));
            structure.AddBone(ColladaToNUIJointMapping("KNEE_RIGHT"), ColladaToNUIJointMapping("HIP_RIGHT"));
            structure.AddBone(ColladaToNUIJointMapping("ANKLE_RIGHT"), ColladaToNUIJointMapping("KNEE_RIGHT"));
            structure.AddBone(ColladaToNUIJointMapping("FOOT_RIGHT"), ColladaToNUIJointMapping("ANKLE_RIGHT"));

            // Define the skeleton in Unity terms.
            NUISkeleton skeleton = new NUISkeleton(structure);

            // Get the hip right and the chest right.
            Vector3 hipRightTranslation = rigData.GetJoint("HIP_RIGHT").LHSWorldTransformationMatrix.GetColumn(3);
            Vector3 hipLeftTranslation = rigData.GetJoint("HIP_LEFT").LHSWorldTransformationMatrix.GetColumn(3);
            Vector3 shoulderRightTranslation = rigData.GetJoint("SHOULDER_RIGHT").LHSWorldTransformationMatrix.GetColumn(3);
            Vector3 shoulderLeftTranslation = rigData.GetJoint("SHOULDER_LEFT").LHSWorldTransformationMatrix.GetColumn(3);
            
            Vector3 hipRight = hipRightTranslation - hipLeftTranslation;
            Vector3 chestRight = shoulderRightTranslation - shoulderLeftTranslation;
            skeleton.SpineBaseRight = rigData.GetJoint("HIP").LHSWorldTransformationMatrix.inverse.MultiplyVector(hipRight);
            skeleton.ChestRight = rigData.GetJoint("SPINE").LHSWorldTransformationMatrix.inverse.MultiplyVector(chestRight);

            foreach(KeyValuePair<string, ColladaJointData> jointData in rigData.JointData)
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

                if(!structure.IsJointAnExtremity(jointType)) // directionToChild is not needed for extremeties.
                {
                    NUIJointType childType = structure.GetChildJoint(jointType);
                    Vector3 child = rigData.GetJoint(NUIJointToColladaMapping(childType)).LHSWorldTransformationMatrix.GetColumn(3);
                    Vector3 parent = currentJoint.LHSWorldTransformationMatrix.GetColumn(3);

                    directionToChild = child - parent;
                }

                if (jointType == NUIJointType.SpineBase) // The Hip is a special case.
                {
                    Vector3 rightHipWorldPosition = rigData.GetJoint("HIP_RIGHT").LHSWorldTransformationMatrix.GetColumn(3);
                    Vector3 leftHipWorldPosition = rigData.GetJoint("HIP_LEFT").LHSWorldTransformationMatrix.GetColumn(3);
                    Vector3 hipWorldPosition = rigData.GetJoint("HIP").LHSWorldTransformationMatrix.GetColumn(3);

                    directionToChild = ((rightHipWorldPosition + leftHipWorldPosition) / 2F) - hipWorldPosition;

                    //float angle = Vector3.Angle(Vector3.up, directionToChild);
                    //directionToChild = Quaternion.AngleAxis(40 + angle, hipRight) * directionToChild;
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

            int tempFrameRateIndex = EditorGUILayout.Popup("Frame Rate", selectedFrameRateIndex, new string[2] { "30 FPS", "60 FPS" });

            if (tempFrameRateIndex != selectedFrameRateIndex)
            {
                selectedFrameRateIndex = tempFrameRateIndex;
                EditorPrefs.SetInt(FRAME_RATE_INDEX_KEY, selectedFrameRateIndex);
            }

            string tempFileName = EditorGUILayout.TextField(new GUIContent("Animation Name","The name of the animation when saved to .dae format."), fileName);

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
                newFileName = CinemaMocapHelper.GetNewFilename(animSaveDestination, newFileName, "dae");
                UnityEngine.Debug.LogWarning(string.Format(NAME_DUPLICATE_ERROR_MSG, appendedFileName, newFileName));
            }

            // Save
            if (transformationType == TransformationType.Matrix)
            {
                ColladaUtility.SaveAnimationData(data, SOURCE_FILE_MATRIX, string.Format(SAVE_DESTINATION_FORMAT, animSaveDestination, newFileName), true);
            }
            else
            {
                ColladaUtility.SaveAnimationData(data, SOURCE_FILE, string.Format(SAVE_DESTINATION_FORMAT, animSaveDestination, newFileName), false);
            }
        }

        public ColladaAnimationData GetColladaAnimation(NUIHumanoidAnimation animation)
        {
            NUISkeleton outputStructure = GetTargetStructure();

            // TODO: Setup animation properly
            ColladaAnimationData colladaAnimation = new ColladaAnimationData(rigData);
            List<float> elapsedTimes = new List<float>();

            Vector3 startingPosition = Vector3.zero;
            Vector3 rigStartingPosition = rigData.GetJoint("HIP").Translation;

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
                ColladaJointData parentJoint = rigData.GetJoint("HIP");
                currentRig.Add(parentJoint.Id, parentJoint);

                foreach (KeyValuePair<string, ColladaJointData> jointData in rigData.JointData)
                {
                    ColladaJointData colladaJointData = rigData.GetJoint(jointData.Key);

                    if (colladaJointData.Id == "HIP")
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