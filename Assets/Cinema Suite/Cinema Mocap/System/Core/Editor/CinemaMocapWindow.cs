using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Editor
{
    /// <summary>
    /// The main window for Cinema Mocap.
    /// </summary>
    public class CinemaMocapWindow : EditorWindow
    {
        // Workflow
        private MocapPipeline mocapPipeline;
        private MocapWorkflow mocapWorkflowPhase = MocapWorkflow.Record;

        // Layouts
        private List<TypeLabelContextData> layoutProfiles = new List<TypeLabelContextData>();
        private int layoutProfileSelection = 0;
        private const string NO_LAYOUT_PROFILES_FOUND_MSG = "No Layout Profiles were found. Cinema Mocap will not work properly.";

        #region UI
        private const string TITLE = "Mocap 2";
        private const string MENU_ITEM = "Window/Cinema Suite/Cinema Mocap 2/Cinema Mocap 2 %#m";

        private GUISkin skin = null;
        private GUIStyle foldoutStyle = null;

        private Texture2D settingsIcon = null;

        private const float width = 300f;
        private const float gap = 4f;
        private const float TOOLBAR_HEIGHT = 17;

        private AnimBool showInputSettings;
        private AnimBool showMappingSettings;
        private AnimBool showOutputSettings;
        private AnimBool showRecordingSettings;

        private Vector2 scrollPosition = new Vector2();
        #endregion

        //#region Developer Mode
        //private bool isDeveloperModeEnabled = false;
        //#endregion

        /// <summary>
        /// Called when the window is opened.
        /// Initializes all variables and sets up the system.
        /// </summary>
        public void Awake()
        {
            // Setup window
#if UNITY_5 && !UNITY_5_0 || UNITY_2017_1_OR_NEWER
        base.titleContent = new GUIContent(TITLE);
#else
            base.title = TITLE;
#endif
            string res_dir = "Cinema Suite/Cinema Mocap/";
            skin = EditorGUIUtility.Load(res_dir + "CinemaMocap_ProSkin.guiskin") as GUISkin;
            this.minSize = new Vector2(680, 400f);

            // Load UI Images
            string settingsIconName = EditorGUIUtility.isProSkin ? "CinemaMocap_SettingsIcon" : "CinemaMocap_SettingsIcon_Personal";
            settingsIcon = (Texture2D)EditorGUIUtility.Load(res_dir + settingsIconName + ".png");
            if (settingsIcon == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("{0} is missing from Resources folder.", settingsIconName));
            }

            layoutProfiles = CinemaMocapLayout.LoadMetaData();
            if (layoutProfiles.Count < 1)
            {
                Debug.LogError(NO_LAYOUT_PROFILES_FOUND_MSG);
                return;
            }

            // Load Preferences
            if (EditorPrefs.HasKey(CinemaMocapSettingsWindow.LayoutKey))
            {
                string label = EditorPrefs.GetString(CinemaMocapSettingsWindow.LayoutKey);
                int result = layoutProfiles.FindIndex(item => item.Label == label);
                layoutProfileSelection = result;
            }
            //if (EditorPrefs.HasKey(CinemaMocapSettingsWindow.DeveloperModeKey))
            //{
            //    isDeveloperModeEnabled = EditorPrefs.GetBool(CinemaMocapSettingsWindow.DeveloperModeKey);
            //}
        }

        /// <summary>
        /// Called when the window is Enabled.
        /// </summary>
        public void OnEnable()
        {
            layoutProfiles = CinemaMocapLayout.LoadMetaData();
            if (layoutProfiles.Count < 1)
            {
                Debug.LogError(NO_LAYOUT_PROFILES_FOUND_MSG);
                return;
            }

            mocapWorkflowPhase = GetUserDefaultWorkflow();


            if (mocapWorkflowPhase == MocapWorkflow.Record)
            {
                mocapPipeline = new RecordPipeline(true);
            }
            else
            {
                mocapPipeline = new ReviewPipeline(true);
            }

            showInputSettings = new AnimBool(true, Repaint);
            showMappingSettings = new AnimBool(true, Repaint);
            showOutputSettings = new AnimBool(true, Repaint);
            showRecordingSettings = new AnimBool(true, Repaint);

            foldoutStyle = skin.FindStyle("SettingsFoldout");
            if (foldoutStyle == null)
            {
                foldoutStyle = skin.FindStyle("box");
            }
        }

        /// <summary>
        /// Update the logic for the window.
        /// </summary>
        protected void Update()
        {
            if (mocapPipeline != null)
            {
                mocapPipeline.Update();
                if (mocapPipeline.IsRepaintRequired)
                {
                    Repaint();
                }
            }
        }

        /// <summary>
        /// Draw the Window's contents
        /// </summary>
        protected void OnGUI()
        {
            // Organize layout
            Rect toolbar = new Rect(0, 0, base.position.width, TOOLBAR_HEIGHT);
            Rect contentArea = new Rect(0, toolbar.yMax, base.position.width, base.position.height - toolbar.height);
            Rect displayAreaBackground = new Rect(width + (gap * 2), contentArea.yMin, contentArea.width - (width + (gap * 2)), contentArea.height);
            Rect displayArea = new Rect(width + (gap * 2) + 8, contentArea.yMin + gap + 6, contentArea.width - (width + (gap * 3)) - 14, contentArea.height - (gap * 2) - 10);

            // Draw toolbar.
            DrawToolbar(toolbar);

            // Draw the Sidebar.
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(width + (gap * 2)));
            GUILayout.Space(4f);

            // Draw the Input settings section.
            DrawInputSettings();

            // Draw the Output setting area.
            DrawOutputSettings();

            // Draw the Mapping settings area.
            DrawMappingSettings();

            // Draw the Recording/Review settings Area.
            if (mocapWorkflowPhase == MocapWorkflow.Record)
            {
                DrawRecordingSettings();
            }
            else if (mocapWorkflowPhase == MocapWorkflow.Review)
            {
                DrawReviewSettings();
            }

            // Close the Sidebar
            Rect rect = EditorGUILayout.GetControlRect();
            GUI.Box(rect, string.Empty, foldoutStyle);
            EditorGUILayout.EndScrollView();

            // Draw display area
#if UNITY_5_5_OR_NEWER
            GUI.Box(displayAreaBackground, string.Empty, "CurveEditorBackground");
#else
            GUI.Box(displayAreaBackground, string.Empty, "AnimationCurveEditorBackground");
#endif
            GUILayout.BeginArea(displayArea);
            mocapPipeline.LayoutProfile.Area = new Rect(0, 0, displayArea.width, displayArea.height);
            mocapPipeline.DrawDisplayArea();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Perform cleanup on window close.
        /// </summary>
        protected void OnDestroy()
        {
            if (mocapPipeline != null)
            {
                mocapPipeline.OnDestroy();
            }
            Resources.UnloadUnusedAssets();
        }

        public MocapWorkflow GetUserDefaultWorkflow()
        {
            //Get workflow phase from default input
            List<InputProfileMetaData> inputProfiles = InputProfile.LoadMetaData(MocapWorkflow.Record, MocapWorkflow.Review);
            // Try to load the user's preferred input method.
            if (EditorPrefs.HasKey(CinemaMocapSettingsWindow.InputProfileKey))
            {
                string label = EditorPrefs.GetString(CinemaMocapSettingsWindow.InputProfileKey);
                int result = inputProfiles.FindIndex(item => item.Attribute.ProfileName == label);
                // Set the correct workflow phase
                return inputProfiles[result].Attribute.MocapPhase;
            }
            return MocapWorkflow.Record;
        }

        /// <summary>
        /// Draw the window's toolbar.
        /// </summary>
        /// <param name="toolbar">The toolbar's area.</param>
        private void DrawToolbar(Rect toolbar)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(toolbar.width));

            //if (isDeveloperModeEnabled)
            {
                MocapWorkflow tempWorkflow = (MocapWorkflow)EditorGUILayout.EnumPopup(mocapWorkflowPhase, EditorStyles.toolbarDropDown);
                if (tempWorkflow != mocapWorkflowPhase)
                {
                    mocapWorkflowPhase = tempWorkflow;

                    if (mocapWorkflowPhase == MocapWorkflow.Record)
                    {
                        mocapPipeline.OnDestroy();
                        mocapPipeline = new RecordPipeline(GetUserDefaultWorkflow() == MocapWorkflow.Record);
                    }
                    else
                    {
                        mocapPipeline.OnDestroy();
                        mocapPipeline = new ReviewPipeline(GetUserDefaultWorkflow() == MocapWorkflow.Review);
                    }
                }
            }

            GUIContent[] content = new GUIContent[layoutProfiles.Count];
            for (int i = 0; i < layoutProfiles.Count; i++)
            {
                content[i] = new GUIContent(layoutProfiles[i].Label);
            }
            GUILayout.FlexibleSpace();

            int tempSelection = EditorGUILayout.Popup(new GUIContent(string.Empty, "Layout"), layoutProfileSelection, content, EditorStyles.toolbarDropDown, GUILayout.Width(120));

            if (layoutProfileSelection != tempSelection || mocapPipeline.LayoutProfile == null)
            {
                mocapPipeline.LayoutProfile = Activator.CreateInstance(layoutProfiles[tempSelection].Type) as CinemaMocapLayout;
                if (mocapPipeline.InputProfile != null)
                {
                    mocapPipeline.LayoutProfile.AspectRatio = mocapPipeline.InputProfile.AspectRatio;
                }
                layoutProfileSelection = tempSelection;
            }

            if (GUILayout.Button(new GUIContent(settingsIcon, "Settings"), EditorStyles.toolbarButton))
            {
                EditorWindow.GetWindow(typeof(CinemaMocapSettingsWindow));
            }

            // Check if the Welcome Window exists and if so, show an icon for it.
            var helpWindowType = Type.GetType("CinemaSuite.CinemaSuiteWelcome");
            if (helpWindowType != null)
            {
                if (GUILayout.Button(new GUIContent("?", "Help"), EditorStyles.toolbarButton))
                {
                    EditorWindow.GetWindow(helpWindowType);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

#region Sidebar UI Sections
        /// <summary>
        /// Draw the Input settings.
        /// </summary>
        private void DrawInputSettings()
        {
            Rect rect = EditorGUILayout.GetControlRect();

            showInputSettings.target = EditorGUI.Foldout(rect, showInputSettings.target, "Input Settings");

#if UNITY_5
        using (var group = new EditorGUILayout.FadeGroupScope(showInputSettings.faded))
        {
            if (group.visible)
            {
#else
            {
                if (showInputSettings.target)
                {
#endif
                    EditorGUI.indentLevel++;

                    mocapPipeline.DrawInputSettings();

                    EditorGUI.indentLevel--;
                    GUILayout.Space(10f);
                }
            }
        }

        /// <summary>
        /// Draw the Mapping Settings UI
        /// </summary>
        private void DrawMappingSettings()
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUI.Box(rect, string.Empty, foldoutStyle);
            showMappingSettings.target = EditorGUI.Foldout(rect, showMappingSettings.target, "Mapping Settings");

#if UNITY_5
        using (var group = new EditorGUILayout.FadeGroupScope(showMappingSettings.faded))
        {
            if (group.visible)
            {
#else
            {
                if (showMappingSettings.target)
                {
#endif
                    EditorGUI.indentLevel++;

                    mocapPipeline.DrawMappingSettings();

                    EditorGUI.indentLevel--;
                    GUILayout.Space(10f);
                }
            }
        }

        /// <summary>
        /// Draw the Animation Settings UI
        /// </summary>
        private void DrawOutputSettings()
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUI.Box(rect, string.Empty, foldoutStyle);
            showOutputSettings.target = EditorGUI.Foldout(rect, showOutputSettings.target, "Output Settings");

#if UNITY_5
        using (var group = new EditorGUILayout.FadeGroupScope(showOutputSettings.faded))
        {
            if (group.visible)
            {
#else
            {
                if (showOutputSettings.target)
                {
#endif
                    EditorGUI.indentLevel++;

                    mocapPipeline.DrawOutputSettings();

                    EditorGUI.indentLevel--;
                    GUILayout.Space(10f);
                }
            }
        }

        /// <summary>
        /// Draw the Recording Settings UI
        /// </summary>
        private void DrawRecordingSettings()
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUI.Box(rect, string.Empty, foldoutStyle);
            showRecordingSettings.target = EditorGUI.Foldout(rect, showRecordingSettings.target, "Recording");

#if UNITY_5
        using (var group = new EditorGUILayout.FadeGroupScope(showRecordingSettings.faded))
        {
            if (group.visible)
            {
#else
            {
                if (showRecordingSettings.target)
                {
#endif
                    EditorGUI.indentLevel++;

                    mocapPipeline.DrawPipelineSettings();

                    EditorGUI.indentLevel--;
                    GUILayout.Space(10f);
                }
            }
        }

        /// <summary>
        /// Draw the Review Settings UI
        /// </summary>
        private void DrawReviewSettings()
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUI.Box(rect, string.Empty, foldoutStyle);
            showRecordingSettings.target = EditorGUI.Foldout(rect, showRecordingSettings.target, "Review");

#if UNITY_5
        using (var group = new EditorGUILayout.FadeGroupScope(showRecordingSettings.faded))
        {
            if (group.visible)
            {
#else
            {
                if (showRecordingSettings.target)
                {
#endif
                    EditorGUI.indentLevel++;

                    mocapPipeline.DrawPipelineSettings();

                    EditorGUI.indentLevel--;
                }

                GUILayout.Space(10f);
            }
        }
#endregion

        /// <summary>
        /// Show the Cinema Mocap Window
        /// </summary>
        [MenuItem(MENU_ITEM, false, 20)]
        private static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(CinemaMocapWindow));

            // Check if we should show the welcome window
            bool showWelcome = true;
            if (EditorPrefs.HasKey("CinemaSuite.WelcomeWindow.ShowOnStartup"))
            {
                showWelcome = EditorPrefs.GetBool("CinemaSuite.WelcomeWindow.ShowOnStartup");
            }

            if (showWelcome)
            {
                // Check if the Welcome Window exists and if so, show it.
                var helpWindowType = Type.GetType("CinemaSuite.CinemaSuiteWelcome");
                if (helpWindowType != null)
                {
                    EditorWindow.GetWindow(helpWindowType);
                }
            }
        }
    }

}