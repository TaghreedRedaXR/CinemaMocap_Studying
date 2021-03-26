using CinemaSuite.CinemaMocap.System.Core.Capture;
using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline
{
    public class RecordPipeline : MocapPipeline
    {
        protected Texture2D recordingImage;

        protected RecordingState captureState = RecordingState.NotRecording;
        protected float delay = 0;

        private int delaySelection = 0;
        private readonly GUIContent[] delays = { new GUIContent("0 Seconds"), new GUIContent("3 Seconds"), new GUIContent("5 Seconds"), new GUIContent("10 Seconds") };

        private DateTime startTime;

        // Animation Capture data
        private MocapSession session;

        private CaptureCache cache;

        //EditorPrefs
        private const string RECORD_START_DELAY_KEY = "CinemaSuite.RecordPipelineStartDelay";

        /// <summary>
        /// Returns the recording state.
        /// </summary>
        public RecordingState RecordingState
        {
            get
            {
                return captureState;
            }
        }

        public RecordPipeline(bool loadFromEditorPrefs) : base(loadFromEditorPrefs)
        {
            loadProfiles(loadFromEditorPrefs);
            cache = new CaptureCache();

            recordingImage = EditorGUIUtility.Load("Cinema Suite/Cinema Mocap/" + "Recording.png") as Texture2D;
            if (recordingImage == null)
            {
                UnityEngine.Debug.LogWarning("Recording Image missing from Resources folder.");
            }

            LoadEditorPrefs();
        }

        public override void Update()
        {
            base.Update();

            // Check if in pre-recording state
            if (captureState == RecordingState.PreRecording)
            {
                var elapsedTime = new TimeSpan(DateTime.Now.Ticks - startTime.Ticks);
                float timeLeft = delay - elapsedTime.Seconds;
                if (timeLeft <= 0)
                {
                    // Begin recording data
                    beginRecording();
                }
            }
        }

        protected override void loadProfiles(bool loadFromEditorPrefs)
        {
            base.loadProfiles(loadFromEditorPrefs, CinemaSuite.CinemaMocap.System.Core.MocapWorkflow.Record);
        }

        public override void DrawInputSettings()
        {
            EditorGUILayout.BeginHorizontal();

            bool isDeviceActive = (InputProfile != null) && InputProfile.IsDeviceOn;
            EditorGUI.BeginDisabledGroup(isDeviceActive);

            GUIContent[] content = new GUIContent[inputProfiles.Count];
            for (int i = 0; i < inputProfiles.Count; i++)
            {
                content[i] = new GUIContent(inputProfiles[i].Attribute.ProfileName);
            }
            int tempSelection = EditorGUILayout.Popup(new GUIContent(INPUT), mocapProfileSelection, content);


            if (mocapProfileSelection != tempSelection || InputProfile == null)
            {
                mocapProfileSelection = tempSelection;

                InputProfile = Activator.CreateInstance(inputProfiles[mocapProfileSelection].Type) as InputProfile;
                InputProfile.FrameCaptured += MocapProfile_SkeletonFrameCaptured;
                InputProfile.InputSkeletonTypeChanged += InputProfile_InputSkeletonTypeChanged;

                // Input Profile changed.
                inputProfileChanged();
            }

            EditorGUI.EndDisabledGroup();


            bool toggleOn = false;
            Color temp = GUI.color;
            if (InputProfile.IsDeviceOn)
            {
                GUI.color = Color.green;
                toggleOn = GUILayout.Toggle(InputProfile.IsDeviceOn, ON, EditorStyles.miniButton, GUILayout.Width(40f));
            }
            else
            {
                GUI.color = Color.red;
                toggleOn = GUILayout.Toggle(InputProfile.IsDeviceOn, OFF, EditorStyles.miniButton, GUILayout.Width(40f));
            }
            GUI.color = temp;

            if (toggleOn && !InputProfile.IsDeviceOn)
            {
                Debug.Log("Cinema Mocap: Starting your device...");
                InputProfile.TurnOnDevice();
                EditorCoroutine.start(WaitForSensorAvailable());
            }
            else if (!toggleOn && InputProfile.IsDeviceOn)
            {
                if (InputProfile != null)
                {
                    if (this.RecordingState != RecordingState.NotRecording)
                    {
                        StopRecording();
                    }
                    InputProfile.TurnOffDevice();
                }

                if (OutputProfile != null)
                {
                    OutputProfile.Reset();
                }
            }

            EditorGUILayout.EndHorizontal();

            InputProfile.DrawInputSettings();
        }

        public override void DrawPipelineSettings()
        {
            int tempDelaySelection = EditorGUILayout.Popup(new GUIContent("Start Delay", "The delay in seconds before recording begins after pressing the record button."), delaySelection, delays);

            if (tempDelaySelection != delaySelection)
            {
                delaySelection = tempDelaySelection;
                EditorPrefs.SetInt(RECORD_START_DELAY_KEY, delaySelection);
            }

            bool isDeveloperModeEnabled = true;
            if (isDeveloperModeEnabled)
            {
                bool tempSaveSession = EditorGUILayout.Toggle(new GUIContent("Save Session", "Saves the raw data of the session in the project folder for later use."), saveSession);

                if (tempSaveSession != saveSession)
                {
                    saveSession = tempSaveSession;
                    EditorPrefs.SetBool(PIPELINE_SAVE_SESSION_KEY, saveSession);
                }

                if (saveSession)
                {
                    // Check if save location has changed from settings window
                    if (sessionSaveDestination != EditorPrefs.GetString(PIPELINE_SAVE_DESTINATION_KEY))
                    {
                        sessionSaveDestination = EditorPrefs.GetString(PIPELINE_SAVE_DESTINATION_KEY);
                        absPath = (Application.dataPath).Replace("Assets", sessionSaveDestination);
                    }

                    string tempFileName = EditorGUILayout.TextField(new GUIContent("Session Filename"), fileName);

                    if (tempFileName != fileName)
                    {
                        fileName = tempFileName;
                        EditorPrefs.SetString(PIPELINE_SAVE_FILENAME_KEY, fileName);
                    }

                    EditorGUILayout.PrefixLabel("Save Location:");

                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.indentLevel++;
                    EditorGUILayout.SelectableLabel(sessionSaveDestination, EditorStyles.textField, GUILayout.Height(16f));
                    EditorGUI.indentLevel--;


                    bool saveDestChanged = false;
                    if (GUILayout.Button(new GUIContent("..."), EditorStyles.miniButton, GUILayout.Width(24f)))
                    {
                        // Ensure the path text field does not have focus, or else we cannot change the contents.
                        GUI.SetNextControlName("");
                        GUI.FocusControl("");

                        string temp = EditorUtility.SaveFolderPanel("Select a folder within your project", sessionSaveDestination, "");

                        // Pressing cancel returns an empty string, don't clear the previous text on cancel.
                        if (temp != string.Empty) absPath = temp;

                        saveDestChanged = true;
                    }

                    EditorGUILayout.EndHorizontal();

                    // Display error if path not within project folder
                    if (!absPath.StartsWith(Application.dataPath))
                    {
                        EditorGUILayout.HelpBox("Invalid selection!\nYou must select a location within your project's \"Assets\" folder.", MessageType.Warning);
                        sessionSaveDestination = CinemaMocapHelper.GetRelativeProjectPath(Application.dataPath);
                    }
                    else
                    {
                        sessionSaveDestination = CinemaMocapHelper.GetRelativeProjectPath(absPath);
                    }

                    if (saveDestChanged)
                    {
                        EditorPrefs.SetString(PIPELINE_SAVE_DESTINATION_KEY, sessionSaveDestination);
                    }
                }
            }
            
            EditorGUI.BeginDisabledGroup(!InputProfile.IsDeviceOn);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button(captureState == RecordingState.NotRecording ? new GUIContent("Record") : new GUIContent("Stop"), EditorStyles.miniButton))
            {
                if (captureState == RecordingState.NotRecording)
                {
                    int delaySeconds = int.Parse(delays[delaySelection].text.Split(' ')[0]);
                    StartRecording(delaySeconds);
                }
                else
                {
                    MocapSession session = StopRecording();
                    saveAnimation(session);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        public override void SaveEditorPrefs()
        {
            EditorPrefs.SetInt(RECORD_START_DELAY_KEY, delaySelection);
        }

        public override void LoadEditorPrefs()
        {
            base.LoadEditorPrefs();
            if (EditorPrefs.HasKey(RECORD_START_DELAY_KEY))
            {
                delaySelection = EditorPrefs.GetInt(RECORD_START_DELAY_KEY, 0);
            }
        }

        public override void DrawDisplayArea()
        {
            base.DrawDisplayArea();

            // Draw the recording overlay on the primary viewer.
            Rect primaryViewerArea = LayoutProfile.GetMainViewer();

            if (captureState == RecordingState.PreRecording)
            {
                var elapsedTime = new TimeSpan(DateTime.Now.Ticks - startTime.Ticks);
                int timeLeft = (int)(((delay * 1000) - elapsedTime.TotalMilliseconds) / 1000) + 1;
                GUIStyle countdownFont = new GUIStyle(EditorStyles.label);
                countdownFont.fontSize = (int)(primaryViewerArea.height / 6);
                countdownFont.normal.textColor = Color.white;
                Vector2 size = countdownFont.CalcSize(new GUIContent(timeLeft.ToString()));
                GUI.Label(new Rect(primaryViewerArea.x + primaryViewerArea.width - (size.x), primaryViewerArea.height + primaryViewerArea.y - size.y, size.x, size.y), timeLeft.ToString(), countdownFont);
            }
            if (captureState == RecordingState.Recording)
            {
                GUI.DrawTexture(new Rect((primaryViewerArea.x + primaryViewerArea.width) - recordingImage.width - 4, (primaryViewerArea.height + primaryViewerArea.y) - recordingImage.height, recordingImage.width, recordingImage.height), recordingImage);
            }
        }

        protected override void spoolUpInputEvents()
        {
            base.spoolUpInputEvents();
            InputProfile.FrameCaptured += MocapProfile_SkeletonFrameCaptured;
        }

        /// <summary>
        /// Start recording motion capture data.
        /// </summary>
        /// <param name="delay">A delay in seconds before recording.</param>
        public void StartRecording(int delay)
        {
            this.startTime = DateTime.Now;
            this.delay = delay;
            this.captureState = RecordingState.PreRecording;
        }

        /// <summary>
        /// Stop the recording.
        /// </summary>
        /// <returns>Returns the humanoid animation.</returns>
        public MocapSession StopRecording()
        {
            // Change to stopped state
            captureState = RecordingState.NotRecording;

            return session;
        }

        private void beginRecording()
        {
            cache = new CaptureCache();
            captureState = RecordingState.Recording;
            session = ScriptableObject.CreateInstance<MocapSession>();

            session.MetaData = InputProfile.GetSessionMetaData();
            session.CaptureData = new List<MocapSessionKeyframe>();

            startTime = DateTime.Now;
        }

        /// <summary>
        /// Receive frame data from the Input profile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void MocapProfile_SkeletonFrameCaptured(object sender, FrameDataEventArgs args)
        {
            if (MappingProfile != null && OutputProfile != null)
            {
                var elapsedTime = new TimeSpan(DateTime.Now.Ticks - startTime.Ticks);
                var skeleton = NUICaptureHelper.GetNUISkeleton(args.SkeletonFrameData);

                // Cache raw data
                cache.AddNewFrame(skeleton, elapsedTime.TotalMilliseconds);

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
                NUISkeleton mapped = MappingProfile.MapSkeleton(skeleton);
                Vector3 position = MappingProfile.GetHipPosition(skeleton);
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

                // Send the mapped and filtered skeleton to the output profile.
                cache.AddResult(filtered);
                OutputProfile.UpdatePreview(filtered, position);

                if (session != null && captureState == RecordingState.Recording)
                {
                    // Add frame to session.
                    MocapSessionKeyframe kf = new MocapSessionKeyframe(args.SkeletonFrameData, (int)elapsedTime.TotalMilliseconds);
                    session.CaptureData.Add(kf);
                }
            }
        }

        /// <summary>
        /// A co-routine that checks for when the sensor becomes available.
        /// </summary>
        protected IEnumerator WaitForSensorAvailable()
        {
            int timeoutMS = 4000;
            long startTime = DateTime.Now.Ticks;
            TimeSpan elapsedTime;

            do
            {
                elapsedTime = new TimeSpan(DateTime.Now.Ticks - startTime);
                if (InputProfile.IsDeviceOn) break;
                yield return null;
            }
            while (elapsedTime.TotalMilliseconds < timeoutMS);

            if (!InputProfile.IsDeviceOn)
            {
                Debug.LogWarning("Cinema Mocap: Device failed to turn on. Ensure drivers are installed and your device is connected properly.");
                InputProfile.TurnOffDevice(); //profile can be "open" even when device isn't plugged in, close it.
            }
            else
            {
                Debug.Log("Cinema Mocap: Device started.");
                this.startTime = DateTime.Now;
            }
        }

    }
}