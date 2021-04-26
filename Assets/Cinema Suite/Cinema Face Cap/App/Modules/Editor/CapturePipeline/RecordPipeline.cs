
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Capture;
using CinemaSuite.CinemaFaceCap.App.Core.Editor;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline
{
    public class RecordPipeline : FaceCapPipeline
    {
        protected Texture2D recordingImage;
        protected Texture2D folderIcon;

        protected RecordingState captureState = RecordingState.NotRecording;
        protected float delay = 0;

        private int delaySelection = 0;
        private readonly GUIContent[] delays = { new GUIContent("0 Seconds"), new GUIContent("3 Seconds"), new GUIContent("5 Seconds"), new GUIContent("10 Seconds") };

        private DateTime startTime;

        // Animation Capture data
        private FaceCapSession session;
        private CaptureCache cache;

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
            cache = new CaptureCache();

            recordingImage = EditorGUIUtility.Load("Cinema Suite/Cinema Face Cap/Recording.png") as Texture2D;
            if (recordingImage == null)
            {
                UnityEngine.Debug.LogWarning("Recording Image missing from Resources folder.");
            }

            if (EditorPrefs.HasKey(CinemaFaceCapSettingsWindow.SessionFolderKey))
            {
                filePath = EditorPrefs.GetString(CinemaFaceCapSettingsWindow.SessionFolderKey);
            }
            else
            {
                filePath = CinemaFaceCapSettingsWindow.SessionFolderDefaultPath;
                EditorPrefs.SetString(CinemaFaceCapSettingsWindow.SessionFolderKey, filePath);
            }


            folderIcon = EditorGUIUtility.Load("Cinema Suite/Cinema Face Cap/cs_Folder.png") as Texture2D;
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
            base.loadProfiles(loadFromEditorPrefs, Workflow.Record);
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
                InputProfile.FrameCaptured += InputProfile_FrameCaptured;
                InputProfile.InputTypeChanged += InputProfile_InputSkeletonTypeChanged;

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
                Debug.Log("Cinema Face Cap: Starting your device...");
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
            delaySelection = EditorGUILayout.Popup(new GUIContent("Start Delay", "The delay in seconds before recording begins after pressing the record button."), delaySelection, delays);

            bool isDeveloperModeEnabled = true;
            if (isDeveloperModeEnabled)
            {
                saveSession = EditorGUILayout.Toggle(new GUIContent("Save Session", "Saves the raw data of the session in the project folder for later use."), saveSession);

                if (saveSession)
                {
                    fileName = EditorGUILayout.TextField(new GUIContent("Session Filename"), fileName);
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
                        EditorPrefs.SetString(CinemaFaceCapSettingsWindow.SessionFolderKey, filePath);
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
                    FaceCapSession session = StopRecording();
                    saveAnimation(session);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
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
            InputProfile.FrameCaptured += InputProfile_FrameCaptured;
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
        public FaceCapSession StopRecording()
        {
            // Change to stopped state
            captureState = RecordingState.NotRecording;

            return session;
        }

        private void beginRecording()
        {
            cache = new CaptureCache();
            captureState = RecordingState.Recording;
            session = ScriptableObject.CreateInstance<FaceCapSession>();

            session.MetaData = InputProfile.GetSessionMetaData();
            session.CaptureData = new List<FaceCapSessionKeyframe>();

            startTime = DateTime.Now;
        }

        /// <summary>
        /// Receive frame data from the Input profile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void InputProfile_FrameCaptured(object sender, FrameDataEventArgs args)
        {
            if (MappingProfile != null && OutputProfile != null)
            {
                var elapsedTime = new TimeSpan(DateTime.Now.Ticks - startTime.Ticks);
                var face = getFaceCapFrame(args.FaceCapFrameData);

                // Cache raw data
                cache.AddNewFrame(face, elapsedTime.TotalMilliseconds);

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

                // Send the mapped and filtered skeleton to the output profile.
                cache.AddResult(filtered);
                OutputProfile.UpdatePreview(filtered);

                if (session != null && captureState == RecordingState.Recording)
                {
                    // Add frame to session.
                    FaceCapSessionKeyframe kf = new FaceCapSessionKeyframe(args.FaceCapFrameData, (int)elapsedTime.TotalMilliseconds);
                    session.CaptureData.Add(kf);
                }
            }
        }

        private MappedFaceCapFrame getFaceCapFrame(FaceCapFrameData faceCapFrameData)
        {
            MappedFaceCapFrame result = new MappedFaceCapFrame();
            result.rotation = faceCapFrameData.FaceOrientation;

            for(int i = 0; i < faceCapFrameData.AnimationUnits.Count; i++)
            {
                result.AnimationUnits.Add((FaceShapeAnimations)i, faceCapFrameData.AnimationUnits[i]);
            }

            return result;
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