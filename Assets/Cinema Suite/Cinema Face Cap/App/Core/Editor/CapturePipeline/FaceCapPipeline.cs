
using BaseSystem = System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.UI;
using CinemaSuite.CinemaFaceCap.App.Core.Capture;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline
{
    public delegate void PipelineEventHandler(object sender, BaseSystem.EventArgs args);

    public abstract class FaceCapPipeline
    {
        #region Profiles
        public InputProfile InputProfile = null;

        public OutputProfile OutputProfile = null;

        public MappingProfile MappingProfile = null;

        public CinemaFaceCapLayout LayoutProfile = null;
        #endregion

        // Mocap Input profile
        protected List<InputProfileMetaData> inputProfiles = new List<InputProfileMetaData>();
        protected int mocapProfileSelection = 0;

        // Output profile
        private List<TypeLabelContextData> outputProfiles = new List<TypeLabelContextData>();
        private int outputProfileSelection = 0;

        // Filters
        private List<CaptureFilter> filters = new List<CaptureFilter>();

        protected List<CaptureFilter> preMapFilters = new List<CaptureFilter>();
        protected List<CaptureFilter> postMapFilters = new List<CaptureFilter>();

        protected event PipelineEventHandler MappingSettingsUpdated;

        #region Language
        protected const string INPUT = "Input";
        protected const string ON = "ON";
        protected const string OFF = "OFF";

        private const string NO_INPUT_PROFILES_FOUND_MSG = "No Input Profiles were found. Cinema Face Cap will not work properly.";
        private const string NO_OUTPUT_PROFILES_FOUND_MSG = "No Output Profiles were found. Cinema Face Cap will not work properly.";
        #endregion

        // Recording Save Directory
        protected const string NAME_DUPLICATE_ERROR_MSG = "{0}.asset exists. Saving as {1}.asset...";
        protected const string SAVE_DESTINATION_FORMAT = "{0}/{1}.asset"; // relative path and filename
        protected string filePath = "Assets";
        protected string fileName = "SavedSession";

        private FaceCapSession session;
        protected bool saveSession = true;

        float moveBtnWidth = 40f;

        public FaceCapPipeline(bool loadFromEditorPrefs)
        {
            loadProfiles(loadFromEditorPrefs);

            // Load Filters
            filters = CaptureFilter.loadAvailableFilters();

            preMapFilters.Clear();
            postMapFilters.Clear();
            foreach (var filter in filters)
            {
                if (filter.PreMapping)
                    preMapFilters.Add(filter);
                else
                    postMapFilters.Add(filter);
            }

            //TODO: Load saved ordinals here

            //Sort lists
            preMapFilters.Sort((x, y) => x.Ordinal.CompareTo(y.Ordinal));
            postMapFilters.Sort((x, y) => x.Ordinal.CompareTo(y.Ordinal));

            spoolUpInputEvents();
        }

        public bool IsRepaintRequired
        {
            get
            {
                return (InputProfile.IsDeviceOn);
            }
        }

        public virtual void Update()
        {
            if (InputProfile != null)
            {
                InputProfile.Update();
            }
        }

        public void OnDestroy()
        {
            if (InputProfile != null)
            {
                InputProfile.Destroy();
            }

            if (OutputProfile != null)
            {
                OutputProfile.Destroy();
            }
        }

        #region OnGUI Sections
        public virtual void DrawInputSettings() { }

        public void DrawMappingSettings()
        {
            bool hasMappingSettingBeenAltered = false;
            // Draw Mapping Settings
            if (MappingProfile != null)
            {
                if(MappingProfile.UpdateParameters())
                {
                    hasMappingSettingBeenAltered = true;
                }
            }

            int swapIndex = -1;
            int swapDir = 0; //1 = up, 2 = down

            // Draw Pre-Mapping filters
            for (int i = 0; i < preMapFilters.Count; i++)
            {
                var filter = preMapFilters[i];

                EditorGUILayout.BeginHorizontal();

                bool temp = EditorGUILayout.Toggle(new GUIContent(filter.Name), filter.Enabled);
                if(temp!= filter.Enabled)
                {
                    filter.Enabled = temp;
                    hasMappingSettingBeenAltered = true;

                    EditorPrefs.SetBool(filter.ENABLED_KEY, temp);
                }

                EditorGUI.BeginDisabledGroup(i == 0);
                if (GUILayout.Button("\u25B2 ", EditorStyles.miniButton, GUILayout.Width(moveBtnWidth)))
                {
                    swapIndex = i;
                    swapDir = 1;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(i == (preMapFilters.Count - 1));
                if (GUILayout.Button("\u25BC ", EditorStyles.miniButton, GUILayout.Width(moveBtnWidth)))
                {
                    swapIndex = i;
                    swapDir = 2;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
                if (filter.Enabled)
                {
                    if (filter.UpdateParameters())
                    {
                        hasMappingSettingBeenAltered = true;
                    }
                }
            }

            //Apply any swaps to preMapFilters
            if (swapIndex != -1)
            {
                if (swapDir == 1) // Move up
                {
                    // Swap filters in list
                    var temp = preMapFilters[swapIndex - 1];
                    preMapFilters[swapIndex - 1] = preMapFilters[swapIndex];
                    preMapFilters[swapIndex] = temp;

                    // Swap ordinal values
                    int tempOrd = preMapFilters[swapIndex - 1].Ordinal;
                    preMapFilters[swapIndex - 1].Ordinal = preMapFilters[swapIndex].Ordinal;
                    preMapFilters[swapIndex].Ordinal = tempOrd;

                    // Save prefs
                    EditorPrefs.SetInt(preMapFilters[swapIndex].ORDINAL_KEY, preMapFilters[swapIndex].Ordinal);
                    EditorPrefs.SetInt(preMapFilters[swapIndex - 1].ORDINAL_KEY, preMapFilters[swapIndex - 1].Ordinal);
                }
                else if (swapDir == 2) // Move down
                {
                    // Swap filters in list
                    var temp = preMapFilters[swapIndex + 1];
                    preMapFilters[swapIndex + 1] = preMapFilters[swapIndex];
                    preMapFilters[swapIndex] = temp;

                    // Swap ordinal values
                    int tempOrd = preMapFilters[swapIndex + 1].Ordinal;
                    preMapFilters[swapIndex + 1].Ordinal = preMapFilters[swapIndex].Ordinal;
                    preMapFilters[swapIndex].Ordinal = tempOrd;

                    // Save prefs
                    EditorPrefs.SetInt(preMapFilters[swapIndex].ORDINAL_KEY, preMapFilters[swapIndex].Ordinal);
                    EditorPrefs.SetInt(preMapFilters[swapIndex + 1].ORDINAL_KEY, preMapFilters[swapIndex + 1].Ordinal);
                }

            }




            swapIndex = -1;
            swapDir = 0; //1 = up, 2 = down
                         // Draw Post-Mapping filters
            for (int i = 0; i < postMapFilters.Count; i++)
            {
                var filter = postMapFilters[i];

                EditorGUILayout.BeginHorizontal();

                var temp = EditorGUILayout.Toggle(new GUIContent(filter.Name), filter.Enabled);
                if (temp != filter.Enabled)
                {
                    filter.Enabled = temp;
                    hasMappingSettingBeenAltered = true;
                }

                EditorGUI.BeginDisabledGroup(i == 0);
                if (GUILayout.Button("\u25B2", EditorStyles.miniButton, GUILayout.Width(moveBtnWidth)))
                {
                    swapIndex = i;
                    swapDir = 1;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(i == (postMapFilters.Count - 1));
                if (GUILayout.Button("\u25BC", EditorStyles.miniButton, GUILayout.Width(moveBtnWidth)))
                {
                    swapIndex = i;
                    swapDir = 2;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
                if (filter.Enabled)
                {
                    if (filter.UpdateParameters())
                    {
                        hasMappingSettingBeenAltered = true;
                    }
                }
            }

            //Apply any swaps to postSwapFilters
            if (swapIndex != -1)
            {
                if (swapDir == 1) // Move up
                {
                    // Swap filters in list
                    var temp = postMapFilters[swapIndex - 1];
                    postMapFilters[swapIndex - 1] = postMapFilters[swapIndex];
                    postMapFilters[swapIndex] = temp;

                    // Swap ordinal values
                    int tempOrd = postMapFilters[swapIndex - 1].Ordinal;
                    postMapFilters[swapIndex - 1].Ordinal = postMapFilters[swapIndex].Ordinal;
                    postMapFilters[swapIndex].Ordinal = tempOrd;

                    // Save prefs
                    EditorPrefs.SetInt(postMapFilters[swapIndex].ORDINAL_KEY, postMapFilters[swapIndex].Ordinal);
                    EditorPrefs.SetInt(postMapFilters[swapIndex - 1].ORDINAL_KEY, postMapFilters[swapIndex - 1].Ordinal);
                }
                else if (swapDir == 2) // Move down
                {
                    // Swap filters in list
                    var temp = postMapFilters[swapIndex + 1];
                    postMapFilters[swapIndex + 1] = postMapFilters[swapIndex];
                    postMapFilters[swapIndex] = temp;

                    // Swap ordinal values
                    int tempOrd = postMapFilters[swapIndex + 1].Ordinal;
                    postMapFilters[swapIndex + 1].Ordinal = postMapFilters[swapIndex].Ordinal;
                    postMapFilters[swapIndex].Ordinal = tempOrd;

                    // Save prefs
                    EditorPrefs.SetInt(postMapFilters[swapIndex].ORDINAL_KEY, postMapFilters[swapIndex].Ordinal);
                    EditorPrefs.SetInt(postMapFilters[swapIndex + 1].ORDINAL_KEY, postMapFilters[swapIndex + 1].Ordinal);
                }
            }


            if (hasMappingSettingBeenAltered && MappingSettingsUpdated != null)
            {
                MappingSettingsUpdated(this, new BaseSystem.EventArgs());
            }
        }

        public void DrawOutputSettings()
        {
            if (outputProfiles.Count == 0 || InputProfile.InputFace == InputFace.None)
            {
                EditorGUILayout.HelpBox("Please select session data first.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                GUIContent[] content = new GUIContent[outputProfiles.Count];
                for (int i = 0; i < outputProfiles.Count; i++)
                {
                    content[i] = new GUIContent(outputProfiles[i].Label);
                }

                int tempOutputSelection = EditorGUILayout.Popup(new GUIContent("Face"), outputProfileSelection, content);

                if (tempOutputSelection != outputProfileSelection || OutputProfile == null)
                {
                    outputProfileSelection = tempOutputSelection;

                    OutputProfile = BaseSystem.Activator.CreateInstance(outputProfiles[outputProfileSelection].Type) as OutputProfile;
                    OutputProfile.Initialize();

                    loadMappingProfiles();
                }
                if (GUILayout.Button("GEN", EditorStyles.miniButton, GUILayout.Width(40f)))
                {
                    createModelPreview();
                }
                EditorGUILayout.EndHorizontal();

                if (OutputProfile != null)
                {
                    OutputProfile.DrawOutputSettings();
                }
            }
        }

        public abstract void DrawPipelineSettings();

        public virtual void DrawDisplayArea()
        {
            InputProfile.DrawDisplayArea(this.LayoutProfile);
        }
#endregion



#region Event Handling
        protected virtual void spoolUpInputEvents()
        {
            InputProfile.InputTypeChanged += InputProfile_InputSkeletonTypeChanged;
        }

        /// <summary>
        /// The Input Profile has a new input skeleton type, we should reload out profiles.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void InputProfile_InputSkeletonTypeChanged(object sender, BaseSystem.EventArgs args)
        {
            inputProfileChanged();
        }

#endregion

        protected virtual void loadProfiles(bool loadFromEditorPrefs)
        { }

        protected void loadProfiles(bool loadFromEditorPrefs, Workflow workflow)
        {
            // Look up user prefs if asked.
            if (loadFromEditorPrefs)
            {
                // Load all the profile meta data.
                loadInputMetaData(Workflow.Record, Workflow.Review);

                // Try to load the user's preferred input method.
                if (EditorPrefs.HasKey(CinemaFaceCapSettingsWindow.InputProfileKey))
                {
                    string label = EditorPrefs.GetString(CinemaFaceCapSettingsWindow.InputProfileKey);
                    int result = inputProfiles.FindIndex(item => item.Attribute.ProfileName == label);

                    // Set the correct workflow phase
                    if (result >= 0)
                    {
                        workflow = inputProfiles[result].Attribute.MocapPhase;
                    }
                }
            }

            // Load the appropriate profiles for the current mocap workflow.
            loadInputMetaData(workflow);
            mocapProfileSelection = 0;

            if (loadFromEditorPrefs)
            {
                if (EditorPrefs.HasKey(CinemaFaceCapSettingsWindow.InputProfileKey))
                {
                    string label = EditorPrefs.GetString(CinemaFaceCapSettingsWindow.InputProfileKey);
                    int result = inputProfiles.FindIndex(item => item.Attribute.ProfileName == label);
                    if (result >= 0)
                    {
                        mocapProfileSelection = result;
                    }
                }
            }

            // Instantiate Input Profile
            InputProfile = BaseSystem.Activator.CreateInstance(inputProfiles[mocapProfileSelection].Type) as InputProfile;
           
            InputProfile.InputTypeChanged += InputProfile_InputSkeletonTypeChanged;

            inputProfileChanged();
        }

        protected virtual void inputProfileChanged()
        {
            if (!loadOutputMetaData())
                return;

            // Instantiate Output profile
            OutputProfile = BaseSystem.Activator.CreateInstance(outputProfiles[outputProfileSelection].Type) as OutputProfile;
            OutputProfile.Initialize();

            // Load the subset of mapping profiles for the selected Input and Output Profiles. Then load the mapper.
            loadMappingProfiles();
        }

        /// <summary>
        /// Load the meta data for all InputProfiles.
        /// </summary>
        private void loadInputMetaData(params Workflow[] workflowFilter)
        {
            // Load Profiles
            inputProfiles = InputProfile.LoadMetaData(workflowFilter);
            if (inputProfiles.Count < 1)
            {
                Debug.LogError(NO_INPUT_PROFILES_FOUND_MSG);
                return;
            }
        }

        /// <summary>
        /// Load the meta data for all output profiles, based on the current InputProfile.
        /// </summary>
        private bool loadOutputMetaData()
        {
            if (InputProfile.InputFace == InputFace.None)
            {
                return false;
            }

            // Get compatible Mapping Profile meta data
            var mappingMetaData = MappingProfile.LoadMetaData(InputProfile.InputFace);

            // Load Output profiles
            List<BaseSystem.Type> outputTypes = mappingMetaData.ConvertAll<BaseSystem.Type>(x => x.Attribute.OutputType);
            outputProfiles = OutputProfile.LoadMetaData(outputTypes);
            if (outputProfiles.Count < 1)
            {
                Debug.LogError(NO_OUTPUT_PROFILES_FOUND_MSG);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Load the mapping profiles compatible with the current input and output profiles and select the first one.
        /// </summary>
        private void loadMappingProfiles()
        {
            if (InputProfile == null || OutputProfile == null)
            {
                return;
            }

            var mappingProfiles = MappingProfile.LoadMappingProfiles(InputProfile.InputFace, OutputProfile.GetType());

            if (mappingProfiles.Count == 0)
            {
                return;
            }

            MappingProfile = mappingProfiles[0];
            MappingProfile.OutputStructure = OutputProfile.GetTargetStructure();
        }
        
        /// <summary>
        /// Once capture is complete, request the OutputProfile to save the animation.
        /// </summary>
        protected void saveAnimation(FaceCapSession session)
        {
            if (OutputProfile == null)
            {
                Debug.LogWarning("No Output method was found. Animation was not saved");
                return;
            }
            if (MappingProfile == null)
            {
                Debug.LogWarning("No Mapping method was found. Animation was not saved");
                return;
            }
            if(session == null)
            {
                return;
            }

            var animation = new MappedFaceCapAnimation();
            var cache = new CaptureCache();

            foreach (var keyframe in session.CaptureData)
            {
                var elapsedTime = keyframe.ElapsedMilliseconds;
                var face = getFaceCapFrame(keyframe.FrameData);

                cache.AddNewFrame(face, elapsedTime);

                // Filter the raw input with enabled filters.
                var filtered = face;
                foreach (var filter in preMapFilters)
                {
                    if (filter.Enabled)
                    {
                        filtered = filter.Filter(cache);
                        cache.AddFiltered(filter.Name, filtered);
                    }
                }

                // Convert the input skeleton to the normalized skeleton (Unity)
                var mapped = MappingProfile.MapFace(filtered);
                cache.AddMapped(mapped);

                // Apply any post-mapped filters selected by the user.
                filtered = mapped;
                foreach (var filter in postMapFilters)
                {
                    if (filter.Enabled)
                    {
                        filtered = filter.Filter(cache);
                        cache.AddFiltered(filter.Name, filtered);
                    }
                }
                cache.AddResult(filtered);

                animation.AddKeyframe(new MappedFaceCapKeyframe(filtered, keyframe.ElapsedMilliseconds));
            }

            // Save the session
            if (saveSession)
            {
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
                if (BaseSystem.IO.File.Exists(string.Format(SAVE_DESTINATION_FORMAT, filePath, newFileName)))
                {
                    newFileName = Utility.Helper.GetNewFilename(filePath, fileName, "asset");
                    UnityEngine.Debug.LogWarning(string.Format(NAME_DUPLICATE_ERROR_MSG, fileName, newFileName));
                }

                AssetDatabase.CreateAsset(session, string.Format(SAVE_DESTINATION_FORMAT, filePath, newFileName));
                AssetDatabase.SaveAssets();
            }

            // Save the animation
            OutputProfile.SaveAnimation(animation);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Request the OutputProfile to create a preview model.
        /// </summary>
        private void createModelPreview()
        {
            if (OutputProfile == null)
            {
                Debug.LogWarning("No Output method was found. Preview could not be created.");
                return;
            }

            OutputProfile.CreatePreview();
        }

        private MappedFaceCapFrame getFaceCapFrame(FaceCapFrameData faceCapFrameData)
        {
            MappedFaceCapFrame result = new MappedFaceCapFrame();
            result.rotation = faceCapFrameData.FaceOrientation;

            for (int i = 0; i < faceCapFrameData.AnimationUnits.Count; i++)
            {
                result.AnimationUnits.Add((FaceShapeAnimations)i, faceCapFrameData.AnimationUnits[i]);
            }

            return result;
        }
    }
}