using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Editor
{
    /// <summary>
    /// Settings window for Cinema Mocap.
    /// </summary>
    public class CinemaMocapSettingsWindow : EditorWindow
    {
        #region Editor Pref Keys
        public const string InputProfileKey = "CinemaMocap.InputProfile";
        public const string OutputKey = "CinemaMocap.OutputProfile";
        public const string Anim20JointSaveDestKey = "CinemaSuite.Output20JointSaveDestination";
        public const string Anim25JointSaveDestKey = "CinemaSuite.Output25JointSaveDestination";
        public const string SessionSaveDestKey = "CinemaSuite.MocapPipelineSaveDestination";
        public const string LayoutKey = "CinemaMocap.LayoutProfile";
        public const string DeveloperModeKey = "CinemaMocap.DeveloperMode";
        #endregion

        #region Language
        private const string TITLE = "Mocap 2 Settings";
        private const string MENU_ITEM = "Window/Cinema Suite/Cinema Mocap 2/Settings";

        #endregion

        private List<TypeLabelContextData> inputProfiles = new List<TypeLabelContextData>();
        private int mocapProfileSelection = 0;

        private List<TypeLabelContextData> layoutProfiles = new List<TypeLabelContextData>();
        private int layoutProfileSelection = 0;

        private string anim20JointSaveDestination = "Assets";
        private string anim20AbsPath = Application.dataPath; // Path returned by the folder selection panel
        private string anim25JointSaveDestination = "Assets";
        private string anim25AbsPath = Application.dataPath;
        private string sessionSaveDestination = "Assets";
        private string sessionAbsPath = Application.dataPath;

        //private bool developerMode;

        /// <summary>
        /// Called when the window is opened.
        /// Initializes all variables and sets up the system.
        /// </summary>
        public void Awake()
        {
#if UNITY_5 && !UNITY_5_0 || UNITY_2017_1_OR_NEWER
        base.titleContent = new GUIContent(TITLE);
#else
            base.title = TITLE;
#endif
            this.minSize = new Vector2(200f, 300f);
        }

        public void OnEnable()
        {
            loadMocapProfiles();
            loadLayoutProfiles();

            if (EditorPrefs.HasKey(InputProfileKey))
            {
                string label = EditorPrefs.GetString(InputProfileKey);
                int result = inputProfiles.FindIndex(item => item.Label == label);
                mocapProfileSelection = result;
            }

            if (EditorPrefs.HasKey(LayoutKey))
            {
                string label = EditorPrefs.GetString(LayoutKey);
                int result = layoutProfiles.FindIndex(item => item.Label == label);
                layoutProfileSelection = result;
            }

            if (EditorPrefs.HasKey(Anim20JointSaveDestKey))
            {
                anim20JointSaveDestination = EditorPrefs.GetString(Anim20JointSaveDestKey);
                anim20AbsPath = (Application.dataPath).Replace("Assets", anim20JointSaveDestination);
            }

            if (EditorPrefs.HasKey(Anim25JointSaveDestKey))
            {
                anim25JointSaveDestination = EditorPrefs.GetString(Anim25JointSaveDestKey);
                anim25AbsPath = (Application.dataPath).Replace("Assets", anim25JointSaveDestination);
            }

            if (EditorPrefs.HasKey(SessionSaveDestKey))
            {
                sessionSaveDestination = EditorPrefs.GetString(SessionSaveDestKey);
                sessionAbsPath = (Application.dataPath).Replace("Assets", sessionSaveDestination);
            }

            //if (EditorPrefs.HasKey(DeveloperModeKey))
            //{
            //    developerMode = EditorPrefs.GetBool(DeveloperModeKey);
            //}
        }

        /// <summary>
        /// Draw the Window's contents
        /// </summary>
        protected void OnGUI()
        {
            // Show Defaults options
            EditorGUILayout.Foldout(true, "Defaults");
            EditorGUI.indentLevel++;

            GUIContent[] mocapProfileContent = new GUIContent[inputProfiles.Count];
            for (int i = 0; i < inputProfiles.Count; i++)
            {
                mocapProfileContent[i] = new GUIContent(inputProfiles[i].Label);
            }
            int tempProfileSelection = EditorGUILayout.Popup(new GUIContent("Device", "Profile"), mocapProfileSelection, mocapProfileContent);
            if (mocapProfileSelection != tempProfileSelection)
            {
                mocapProfileSelection = tempProfileSelection;
                EditorPrefs.SetString(InputProfileKey, inputProfiles[mocapProfileSelection].Label);
            }

            GUIContent[] layoutContent = new GUIContent[layoutProfiles.Count];
            for (int i = 0; i < layoutProfiles.Count; i++)
            {
                layoutContent[i] = new GUIContent(layoutProfiles[i].Label);
            }
            int tempSelection = EditorGUILayout.Popup(new GUIContent("Layout", "Layout"), layoutProfileSelection, layoutContent);
            if (tempSelection != layoutProfileSelection)
            {
                layoutProfileSelection = tempSelection;
                EditorPrefs.SetString(LayoutKey, layoutProfiles[layoutProfileSelection].Label);
            }

            // Check if save locations have changed from mocap window
            if (anim20JointSaveDestination != EditorPrefs.GetString(Anim20JointSaveDestKey))
            {
                anim20JointSaveDestination = EditorPrefs.GetString(Anim20JointSaveDestKey);
                anim20AbsPath = (Application.dataPath).Replace("Assets", anim20JointSaveDestination);
            }
            if (anim25JointSaveDestination != EditorPrefs.GetString(Anim25JointSaveDestKey))
            {
                anim25JointSaveDestination = EditorPrefs.GetString(Anim25JointSaveDestKey);
                anim25AbsPath = (Application.dataPath).Replace("Assets", anim25JointSaveDestination);
            }
            if (sessionSaveDestination != EditorPrefs.GetString(Anim20JointSaveDestKey))
            {
                sessionSaveDestination = EditorPrefs.GetString(SessionSaveDestKey);
                sessionAbsPath = (Application.dataPath).Replace("Assets", sessionSaveDestination);
            }

            // 20 Joint Save Path Field & Button

            EditorGUILayout.PrefixLabel("Kinect 1 Anim Save:");

            EditorGUILayout.BeginHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.SelectableLabel(anim20JointSaveDestination, EditorStyles.textField, GUILayout.Height(16f));
            EditorGUI.indentLevel--;


            if (GUILayout.Button(new GUIContent("..."), EditorStyles.miniButton, GUILayout.Width(24f)))
            {
                // Ensure the path text field does not have focus, or else we cannot change the contents.
                GUI.SetNextControlName("");
                GUI.FocusControl("");

                string temp = EditorUtility.SaveFolderPanel("Select a folder within your project", anim20JointSaveDestination, "");

                // Pressing cancel returns an empty string, don't clear the previous text on cancel.
                if (temp != string.Empty) anim20AbsPath = temp;
            }

            EditorGUILayout.EndHorizontal();

            // Display error if path not within project folder
            if (!anim20AbsPath.StartsWith(Application.dataPath))
            {
                EditorGUILayout.HelpBox("Invalid selection!\nYou must select a location within your project's \"Assets\" folder.", MessageType.Warning);
                anim20JointSaveDestination = CinemaMocapHelper.GetRelativeProjectPath(Application.dataPath);
                EditorPrefs.SetString(Anim20JointSaveDestKey, anim20JointSaveDestination);
            }
            else
            {
                anim20JointSaveDestination = CinemaMocapHelper.GetRelativeProjectPath(anim20AbsPath);
                EditorPrefs.SetString(Anim20JointSaveDestKey, anim20JointSaveDestination);
            }

            // 25 Joint Save Path Field & Button

            EditorGUILayout.PrefixLabel("Kinect 2 Anim Save:");

            EditorGUILayout.BeginHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.SelectableLabel(anim25JointSaveDestination, EditorStyles.textField, GUILayout.Height(16f));
            EditorGUI.indentLevel--;


            if (GUILayout.Button(new GUIContent("..."), EditorStyles.miniButton, GUILayout.Width(24f)))
            {
                // Ensure the path text field does not have focus, or else we cannot change the contents.
                GUI.SetNextControlName("");
                GUI.FocusControl("");

                string temp = EditorUtility.SaveFolderPanel("Select a folder within your project", anim25JointSaveDestination, "");

                // Pressing cancel returns an empty string, don't clear the previous text on cancel.
                if (temp != string.Empty) anim25AbsPath = temp;
            }

            EditorGUILayout.EndHorizontal();

            // Display error if path not within project folder
            if (!anim25AbsPath.StartsWith(Application.dataPath))
            {
                EditorGUILayout.HelpBox("Invalid selection!\nYou must select a location within your project's \"Assets\" folder.", MessageType.Warning);
                anim25JointSaveDestination = CinemaMocapHelper.GetRelativeProjectPath(Application.dataPath);
                EditorPrefs.SetString(Anim25JointSaveDestKey, anim25JointSaveDestination);
            }
            else
            {
                anim25JointSaveDestination = CinemaMocapHelper.GetRelativeProjectPath(anim25AbsPath);
                EditorPrefs.SetString(Anim25JointSaveDestKey, anim25JointSaveDestination);
            }

            // Session Save Path Field & Button

            EditorGUILayout.PrefixLabel("Session Save:");

            EditorGUILayout.BeginHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.SelectableLabel(sessionSaveDestination, EditorStyles.textField, GUILayout.Height(16f));
            EditorGUI.indentLevel--;


            if (GUILayout.Button(new GUIContent("..."), EditorStyles.miniButton, GUILayout.Width(24f)))
            {
                // Ensure the path text field does not have focus, or else we cannot change the contents.
                GUI.SetNextControlName("");
                GUI.FocusControl("");

                string temp = EditorUtility.SaveFolderPanel("Select a folder within your project", sessionSaveDestination, "");

                // Pressing cancel returns an empty string, don't clear the previous text on cancel.
                if (temp != string.Empty) sessionAbsPath = temp;
            }

            EditorGUILayout.EndHorizontal();

            // Display error if path not within project folder
            if (!sessionAbsPath.StartsWith(Application.dataPath))
            {
                EditorGUILayout.HelpBox("Invalid selection!\nYou must select a location within your project's \"Assets\" folder.", MessageType.Warning);
                sessionSaveDestination = CinemaMocapHelper.GetRelativeProjectPath(Application.dataPath);
                EditorPrefs.SetString(SessionSaveDestKey, sessionSaveDestination);
            }
            else
            {
                sessionSaveDestination = CinemaMocapHelper.GetRelativeProjectPath(sessionAbsPath);
                EditorPrefs.SetString(SessionSaveDestKey, sessionSaveDestination);
            }

            EditorGUI.indentLevel--;

            //EditorGUILayout.Foldout(true, "Developers");

            //EditorGUI.indentLevel++;
            //bool tempDevMode = EditorGUILayout.Toggle(new GUIContent("Developer Mode"), developerMode);
            //if (tempDevMode != developerMode)
            //{
            //    developerMode = tempDevMode;
            //    EditorPrefs.SetBool(DeveloperModeKey, tempDevMode);
            //}
            //EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Perform cleanup on window close
        /// </summary>
        protected void OnDestroy()
        { }

        /// <summary>
        /// Load all of the mocap profiles found in the project.
        /// </summary>
        private void loadMocapProfiles()
        {
            inputProfiles.Clear();

            List<Type> types = CinemaMocapHelper.GetInputProfiles();
            foreach (Type t in types)
            {
                foreach (InputProfileAttribute attribute in t.GetCustomAttributes(typeof(InputProfileAttribute), true))
                {
                    inputProfiles.Add(new TypeLabelContextData(t, attribute.ProfileName));
                }
            }
        }

        /// <summary>
        /// Load all of the layout profiles found in the project.
        /// </summary>
        private void loadLayoutProfiles()
        {
            layoutProfiles.Clear();

            List<Type> types = CinemaMocapHelper.GetLayoutProfiles();
            foreach (Type t in types)
            {
                foreach (CinemaMocapLayoutAttribute attribute in t.GetCustomAttributes(typeof(CinemaMocapLayoutAttribute), true))
                {
                    layoutProfiles.Add(new TypeLabelContextData(t, attribute.Name, attribute.Ordinal));
                }
            }

            layoutProfiles.Sort(delegate (TypeLabelContextData x, TypeLabelContextData y)
            {
                return x.Ordinal - y.Ordinal;
            });
        }

        /// <summary>
        /// Show the Cinema Mocap Window
        /// </summary>
        [MenuItem(MENU_ITEM, false, 200)]
        private static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(CinemaMocapSettingsWindow));
        }
    }
}