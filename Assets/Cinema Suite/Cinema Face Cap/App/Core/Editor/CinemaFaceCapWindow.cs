
using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.UI;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor
{
    /// <summary>
    /// The main window for Cinema Mocap.
    /// </summary>
    public class CinemaFaceCapWindow : EditorWindow
    {
        // Workflow
        private FaceCapPipeline pipeline;
        private Workflow workflowPhase = Workflow.Record;

        // Layout profile
        private List<TypeLabelContextData> layoutProfiles = new List<TypeLabelContextData>();
        private int layoutProfileSelection = 0;
        private const string NO_LAYOUT_PROFILES_FOUND_MSG = "No Layout Profiles were found. Cinema Face Cap will not work properly.";
        
        #region UI
        private const string TITLE = "Face Cap";
        private const string MENU_ITEM = "Window/Cinema Suite/Cinema Face Cap/Cinema Face Cap %#c";
        
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
            if (workflowPhase == Workflow.Record)
            {
                pipeline = new RecordPipeline(true);
            }
            else
            {
                pipeline = new ReviewPipeline(true);
            }
        }

        /// <summary>
        /// Called when the window is Enabled.
        /// </summary>
        public void OnEnable()
        {

            skin = EditorGUIUtility.isProSkin ?
                EditorGUIUtility.Load("Cinema Suite/Cinema Face Cap/CinemaFaceCap_ProSkin.guiskin") as GUISkin :
                EditorGUIUtility.Load("Cinema Suite/Cinema Face Cap/CinemaFaceCap_PersonalSkin.guiskin") as GUISkin;
            this.minSize = new Vector2(680, 400f);

            // Load UI Images
            settingsIcon = skin.FindStyle("SettingsIcon").normal.background;
            if (settingsIcon == null)
            {
                UnityEngine.Debug.LogWarning("CinemaSuite_SettingsIcon.png is missing from Resources folder.");
            }

            foldoutStyle = skin.FindStyle("SettingsFoldout");
            if (foldoutStyle == null)
            {
                foldoutStyle = skin.FindStyle("box");
            }

            layoutProfiles = CinemaFaceCapLayout.LoadMetaData();
            if (layoutProfiles.Count < 1)
            {
                Debug.LogError(NO_LAYOUT_PROFILES_FOUND_MSG);
                return;
            }

            if (workflowPhase == Workflow.Record)
            {
                pipeline = new RecordPipeline(false);
            }
            else
            {
                pipeline = new ReviewPipeline(false);
            }

            showInputSettings = new AnimBool(true, Repaint);
            showMappingSettings = new AnimBool(true, Repaint);
            showOutputSettings = new AnimBool(true, Repaint);
            showRecordingSettings = new AnimBool(true, Repaint);

        }

        /// <summary>
        /// Update the logic for the window.
        /// </summary>
        protected void Update()
        {
            if (pipeline != null)
            {
                pipeline.Update();
                if (pipeline.IsRepaintRequired)
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
            if (workflowPhase == Workflow.Record)
            {
                DrawRecordingSettings();
            }
            else if (workflowPhase == Workflow.Review)
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
            pipeline.LayoutProfile.Area = new Rect(0, 0, displayArea.width, displayArea.height);
            pipeline.DrawDisplayArea();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Perform cleanup on window close.
        /// </summary>
        protected void OnDestroy()
        {
            if (pipeline != null)
            {
                pipeline.OnDestroy();
            }
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Draw the window's toolbar.
        /// </summary>
        /// <param name="toolbar">The toolbar's area.</param>
        private void DrawToolbar(Rect toolbar)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(toolbar.width));

            var tempWorkflow = (Workflow) EditorGUILayout.EnumPopup(workflowPhase, EditorStyles.toolbarDropDown);
            if (tempWorkflow != workflowPhase)
            {
                workflowPhase = tempWorkflow;

                if (workflowPhase == Workflow.Record)
                {
                    pipeline = new RecordPipeline(false);
                }
                else
                {
                    pipeline = new ReviewPipeline(false);
                }
            }


            GUIContent[] content = new GUIContent[layoutProfiles.Count];
            for (int i = 0; i < layoutProfiles.Count; i++)
            {
                content[i] = new GUIContent(layoutProfiles[i].Label);
            }
            GUILayout.FlexibleSpace();

            int tempSelection = EditorGUILayout.Popup(new GUIContent(string.Empty, "Layout"), layoutProfileSelection, content, EditorStyles.toolbarDropDown, GUILayout.Width(120));

            if (layoutProfileSelection != tempSelection || pipeline.LayoutProfile == null)
            {
                pipeline.LayoutProfile = Activator.CreateInstance(layoutProfiles[tempSelection].Type) as CinemaFaceCapLayout;
                if (pipeline.InputProfile != null)
                {
                    pipeline.LayoutProfile.AspectRatio = pipeline.InputProfile.AspectRatio;
                }
                layoutProfileSelection = tempSelection;
            }
            if (GUILayout.Button(new GUIContent(settingsIcon, "Settings"), EditorStyles.toolbarButton))
            {
                GetWindow(typeof(CinemaFaceCapSettingsWindow));
            }

            // Check if the Welcome Window exists and if so, show an icon for it.
            var helpWindowType = Type.GetType("CinemaSuite.CinemaSuiteWelcome");
            if (helpWindowType != null)
            {
                if (GUILayout.Button(new GUIContent("?", "Help"), EditorStyles.toolbarButton))
                {
                    GetWindow(helpWindowType);
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

                    pipeline.DrawInputSettings();

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

                    pipeline.DrawMappingSettings();

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
                    pipeline.DrawOutputSettings();
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

                    pipeline.DrawPipelineSettings();

                    EditorGUI.indentLevel--;
                    GUILayout.Space(10f);
                }
            }
        }

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

                    pipeline.DrawPipelineSettings();

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
            GetWindow(typeof(CinemaFaceCapWindow));

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
                    GetWindow(helpWindowType);
                }
            }
        }
    }
}