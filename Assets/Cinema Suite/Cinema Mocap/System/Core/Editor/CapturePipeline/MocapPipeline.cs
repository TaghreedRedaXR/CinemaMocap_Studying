using CinemaSuite.CinemaMocap.System.Core.Capture;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using BaseSystem = System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline
{
    public delegate void PipelineEventHandler(object sender, BaseSystem.EventArgs args);

    public abstract class MocapPipeline
    {
        #region Profiles
        public InputProfile InputProfile = null;

        public OutputProfile OutputProfile = null;

        public MappingProfile MappingProfile = null;

        public CinemaMocapLayout LayoutProfile = null;
        #endregion

        // Mocap Input profile
        protected List<InputProfileMetaData> inputProfiles = new List<InputProfileMetaData>();
        protected int mocapProfileSelection = 0;

        // Output profile
        private List<TypeLabelContextData> outputProfiles = new List<TypeLabelContextData>();
        private int outputProfileSelection = 0;

        // Filters
        private List<MocapFilter> filters = new List<MocapFilter>();

        protected List<MocapFilter> preMapFilters = new List<MocapFilter>();
        protected List<MocapFilter> postMapFilters = new List<MocapFilter>();

        protected event PipelineEventHandler MappingSettingsUpdated;

        #region Language
        protected const string INPUT = "Input";
        protected const string ON = "ON";
        protected const string OFF = "OFF";

        private const string NO_INPUT_PROFILES_FOUND_MSG = "No Input Profiles were found. Cinema Mocap will not work properly.";
        private const string NO_OUTPUT_PROFILES_FOUND_MSG = "No Output Profiles were found. Cinema Mocap will not work properly.";
        #endregion

        // Recording Save Directory
        protected const string ASSETS_REL_PATH = "Assets";
        protected const string NAME_DUPLICATE_ERROR_MSG = "{0}.asset exists. Saving as {1}.asset...";
        protected const string SAVE_DESTINATION_FORMAT = "{0}/{1}.asset"; // relative path and filename
        protected string sessionSaveDestination = "Assets";
        protected string absPath = Application.dataPath; // Path returned by the folder selection panel
        protected string fileName = "SavedSession";

        // EditorPrefs
        protected const string PIPELINE_SAVE_SESSION_KEY = "CinemaSuite.MocapPipelineSaveSession";
        protected const string PIPELINE_SAVE_FILENAME_KEY = "CinemaSuite.MocapPipelineFilename";
        protected const string PIPELINE_SAVE_DESTINATION_KEY = "CinemaSuite.MocapPipelineSaveDestination";

        private MocapSession session;
        protected bool saveSession = true;

        float moveBtnWidth = 40f;

        public MocapPipeline(bool loadFromEditorPrefs)
        {
            loadProfiles(loadFromEditorPrefs);

            // Load Filters
            filters = MocapFilter.loadAvailableFilters();

            preMapFilters.Clear();
            postMapFilters.Clear();
            foreach (MocapFilter filter in filters)
            {
                if (filter.PreMapping)
                    preMapFilters.Add(filter);
                else
                    postMapFilters.Add(filter);
            }

            LoadEditorPrefs();

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

        public virtual void OnDestroy()
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
                MocapFilter filter = preMapFilters[i];

                EditorGUILayout.BeginHorizontal();

                bool temp = EditorGUILayout.Toggle(new GUIContent(filter.Name), filter.Enabled);
                if(temp!= filter.Enabled)
                {
                    filter.Enabled = temp;
                    hasMappingSettingBeenAltered = true;
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
                    MocapFilter temp = preMapFilters[swapIndex - 1];
                    preMapFilters[swapIndex - 1] = preMapFilters[swapIndex];
                    preMapFilters[swapIndex] = temp;
                }
                else if (swapDir == 2) // Move down
                {
                    MocapFilter temp = preMapFilters[swapIndex + 1];
                    preMapFilters[swapIndex + 1] = preMapFilters[swapIndex];
                    preMapFilters[swapIndex] = temp;
                }
            }




            swapIndex = -1;
            swapDir = 0; //1 = up, 2 = down
                         // Draw Post-Mapping filters
            for (int i = 0; i < postMapFilters.Count; i++)
            {
                MocapFilter filter = postMapFilters[i];

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
                    MocapFilter temp = postMapFilters[swapIndex - 1];
                    postMapFilters[swapIndex - 1] = postMapFilters[swapIndex];
                    postMapFilters[swapIndex] = temp;
                }
                else if (swapDir == 2) // Move down
                {
                    MocapFilter temp = postMapFilters[swapIndex + 1];
                    postMapFilters[swapIndex + 1] = postMapFilters[swapIndex];
                    postMapFilters[swapIndex] = temp;
                }
            }


            if (hasMappingSettingBeenAltered && MappingSettingsUpdated != null)
            {
                MappingSettingsUpdated(this, new BaseSystem.EventArgs());
            }
        }

        public void DrawOutputSettings()
        {
            if (outputProfiles.Count == 0 || InputProfile.InputSkeleton == InputSkeletonType.None)
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

                int tempOutputSelection = EditorGUILayout.Popup(new GUIContent("Skeleton"), outputProfileSelection, content);

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
            InputProfile.InputSkeletonTypeChanged += InputProfile_InputSkeletonTypeChanged;
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

        protected void loadProfiles(bool loadFromEditorPrefs, MocapWorkflow mocapWorkflow)
        {
            // Look up user prefs if asked.
            if (loadFromEditorPrefs)
            {
                // Load all the profile meta data.
                loadInputMetaData(MocapWorkflow.Record, MocapWorkflow.Review);

                // Try to load the user's preferred input method.
                if (EditorPrefs.HasKey(CinemaMocapSettingsWindow.InputProfileKey))
                {
                    string label = EditorPrefs.GetString(CinemaMocapSettingsWindow.InputProfileKey);
                    int result = inputProfiles.FindIndex(item => item.Attribute.ProfileName == label);

                    // Set the correct workflow phase
                    mocapWorkflow = inputProfiles[result].Attribute.MocapPhase;
                }
            }

            // Load the appropriate profiles for the current mocap workflow.
            loadInputMetaData(mocapWorkflow);
            mocapProfileSelection = 0;

            if (loadFromEditorPrefs)
            {
                if (EditorPrefs.HasKey(CinemaMocapSettingsWindow.InputProfileKey))
                {
                    string label = EditorPrefs.GetString(CinemaMocapSettingsWindow.InputProfileKey);
                    int result = inputProfiles.FindIndex(item => item.Attribute.ProfileName == label);
                    mocapProfileSelection = result;
                }
            }

            // Instantiate Input Profile
            InputProfile = BaseSystem.Activator.CreateInstance(inputProfiles[mocapProfileSelection].Type) as InputProfile;
            //InputProfile.SkeletonFrameCaptured += MocapProfile_SkeletonFrameCaptured;
            ////InputProfile.InputSkeletonTypeChanged += InputProfile_InputSkeletonTypeChanged;
            spoolUpInputEvents();

            inputProfileChanged();
        }

        /// <summary>
        /// Save the current field values
        /// </summary>
        public virtual void SaveEditorPrefs()
        {
            EditorPrefs.SetBool(PIPELINE_SAVE_SESSION_KEY, saveSession);
            EditorPrefs.SetString(PIPELINE_SAVE_FILENAME_KEY, fileName);
            EditorPrefs.SetString(PIPELINE_SAVE_DESTINATION_KEY, sessionSaveDestination);
        }

        /// <summary>
        /// Load any saved field values
        /// </summary>
        public virtual void LoadEditorPrefs()
        {
            if (EditorPrefs.HasKey(PIPELINE_SAVE_SESSION_KEY))
            {
                saveSession = EditorPrefs.GetBool(PIPELINE_SAVE_SESSION_KEY, false);
            }
            if (EditorPrefs.HasKey(PIPELINE_SAVE_FILENAME_KEY))
            {
                fileName = EditorPrefs.GetString(PIPELINE_SAVE_FILENAME_KEY, "SavedSession");
            }
            if (EditorPrefs.HasKey(PIPELINE_SAVE_DESTINATION_KEY))
            {
                sessionSaveDestination = EditorPrefs.GetString(PIPELINE_SAVE_DESTINATION_KEY, "Assets");
                absPath = (Application.dataPath).Replace("Assets", sessionSaveDestination);
            }
        }

        protected void inputProfileChanged()
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
        private void loadInputMetaData(params MocapWorkflow[] workflowFilter)
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
            if (InputProfile.InputSkeleton == InputSkeletonType.None)
            {
                return false;
            }

            // Get compatible Mapping Profile meta data
            var mappingMetaData = MappingProfile.LoadMetaData(InputProfile.InputSkeleton);

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

            var mappingProfiles = MappingProfile.LoadMappingProfiles(InputProfile.InputSkeleton, OutputProfile.GetType());

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
        protected void saveAnimation(MocapSession session)
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

            var animation = new NUIHumanoidAnimation();
            var cache = new CaptureCache();

            foreach (MocapSessionKeyframe keyframe in session.CaptureData)
            {
                var elapsedTime = keyframe.ElapsedMilliseconds;
                var skeleton = NUICaptureHelper.GetNUISkeleton(keyframe.Skeleton);

                cache.AddNewFrame(skeleton, elapsedTime);

                // Filter the raw input with enabled filters.
                NUISkeleton filtered = skeleton;
                foreach (MocapFilter filter in preMapFilters)
                {
                    if (filter.Enabled)
                    {
                        filtered = filter.Filter(cache);
                        cache.AddFiltered(filter.Name, filtered);
                    }
                }

                // Convert the input skeleton to the normalized skeleton (Unity)
                NUISkeleton mapped = MappingProfile.MapSkeleton(filtered);
                cache.AddMapped(mapped);

                // Apply any post-mapped filters selected by the user.
                filtered = mapped;
                foreach (MocapFilter filter in postMapFilters)
                {
                    if (filter.Enabled)
                    {
                        filtered = filter.Filter(cache);
                        cache.AddFiltered(filter.Name, filtered);
                    }
                }
                cache.AddResult(filtered);

                filtered.Joints[NUIJointType.SpineBase].Position = skeleton.Joints[NUIJointType.SpineBase].Position;
                animation.AddKeyframe(filtered, keyframe.ElapsedMilliseconds);
            }

            // Save the session
            if (saveSession)
            {
                string newFileName = fileName;
                if (BaseSystem.IO.File.Exists(string.Format(SAVE_DESTINATION_FORMAT, sessionSaveDestination, newFileName)))
                {
                    newFileName = CinemaMocapHelper.GetNewFilename(sessionSaveDestination, fileName, "asset");
                    UnityEngine.Debug.LogWarning(string.Format(NAME_DUPLICATE_ERROR_MSG, fileName, newFileName));
                }

                AssetDatabase.CreateAsset(session, string.Format(SAVE_DESTINATION_FORMAT, sessionSaveDestination, newFileName));
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

    }
}