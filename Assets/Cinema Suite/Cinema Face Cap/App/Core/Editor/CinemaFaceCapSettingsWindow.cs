using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor
{
    public class CinemaFaceCapSettingsWindow : EditorWindow
    {
        #region UI
        private bool wizardFoldout = true;
        private bool saveLocationsFoldout = true;
        private Texture2D folderIcon;
        #endregion

        #region Language
        private const string TITLE = "Face Cap Settings";
        private const string MENU_ITEM = "Window/Cinema Suite/Cinema Face Cap/Settings";

        GUIContent saveLocationsFoldoutContent = new GUIContent("Save Locations");
        GUIContent wizardFoldoutContent = new GUIContent("Wizards");
        #endregion
        
        public static string InputProfileKey { get; internal set; }
        public static string AnimationFolderKey { get { return "CinemaSuite.Output.Animations.Folder"; } }
        public static string SessionFolderKey { get { return "CinemaSuite.Output.Sessions.Folder"; } }
        public static string ExtensionFolderKey { get { return "CinemaSuite.Extensions.CinemaFaceCap.Folder"; } }

        public const string AnimationFolderDefaultPath = "Assets/Animations";
        public const string SessionFolderDefaultPath = "Assets/Animation Sessions";
        public const string ExtensionFolderDefaultPath = "Assets/Extensions/Cinema Face Cap/Editor";

        private string animationFolderPath = AnimationFolderDefaultPath;
        private string sessionFolderPath = SessionFolderDefaultPath;
        private string extensionFolderPath = ExtensionFolderDefaultPath;

        /// <summary>
        /// Awake is called when the window opens for the first time.
        /// </summary>
        public void Awake()
        {
#if UNITY_5 && !UNITY_5_0 || UNITY_2017_1_OR_NEWER
            base.titleContent = new GUIContent(TITLE);
#else
            base.title = TITLE;
#endif
            this.minSize = new Vector2(200f, 300f);
            folderIcon = EditorGUIUtility.Load("Cinema Suite/Cinema Face Cap/cs_Folder.png") as Texture2D;

            if (EditorPrefs.HasKey(AnimationFolderKey))
            {
                animationFolderPath = EditorPrefs.GetString(AnimationFolderKey);
            }
            else
            {
                EditorPrefs.SetString(AnimationFolderKey, animationFolderPath);
            }

            if (EditorPrefs.HasKey(SessionFolderKey))
            {
                sessionFolderPath = EditorPrefs.GetString(SessionFolderKey);
            }
            else
            {
                EditorPrefs.SetString(SessionFolderKey, sessionFolderPath);
            }

            if (EditorPrefs.HasKey(ExtensionFolderKey))
            {
                extensionFolderPath = EditorPrefs.GetString(ExtensionFolderKey);
            }
            else
            {
                EditorPrefs.SetString(ExtensionFolderKey, extensionFolderPath);
            }

        }

        public void OnEnable()
        { }

        protected void OnGUI()
        {
            saveLocationsFoldout = EditorGUILayout.Foldout(saveLocationsFoldout, new GUIContent(saveLocationsFoldoutContent));
            if (saveLocationsFoldout)
            {
                EditorGUI.indentLevel++;

                // Animations folder
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Animation Folder", "The folder where newly created animation files will be placed by default."));

                EditorGUILayout.SelectableLabel(animationFolderPath, EditorStyles.textField, GUILayout.Height(16f));

                bool animDestChanged = false;
                if (GUILayout.Button(folderIcon, EditorStyles.miniButton, GUILayout.Width(32f), GUILayout.Height(16f)))
                {
                    GUI.SetNextControlName("");
                    GUI.FocusControl("");

                    string temp = EditorUtility.SaveFolderPanel("Select a folder within your project", animationFolderPath, "");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        if (!temp.StartsWith(Application.dataPath))
                        {
                            animationFolderPath = "Assets";
                        }
                        else
                        {
                            animationFolderPath = "Assets" + temp.Replace(Application.dataPath, string.Empty);
                        }
                    }
                    animDestChanged = true;
                }
                EditorGUILayout.EndHorizontal();

                if (animDestChanged)
                {
                    EditorPrefs.SetString(AnimationFolderKey, animationFolderPath);
                }

                // Sessions folder
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Session Folder", "The folder where newly created raw data session files will be placed by default."));

                EditorGUILayout.SelectableLabel(sessionFolderPath, EditorStyles.textField, GUILayout.Height(16f));

                bool sessDestChanged = false;
                if (GUILayout.Button(folderIcon, EditorStyles.miniButton, GUILayout.Width(32f), GUILayout.Height(16f)))
                {
                    GUI.SetNextControlName("");
                    GUI.FocusControl("");

                    string temp = EditorUtility.SaveFolderPanel("Select a folder within your project", sessionFolderPath, "");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        if (!temp.StartsWith(Application.dataPath))
                        {
                            sessionFolderPath = "Assets";
                        }
                        else
                        {
                            sessionFolderPath = "Assets" + temp.Replace(Application.dataPath, string.Empty);
                        }
                    }

                    sessDestChanged = true;
                }
                EditorGUILayout.EndHorizontal();

                if (sessDestChanged)
                {
                    EditorPrefs.SetString(SessionFolderKey, sessionFolderPath);
                }

                // Extensions folder
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Extension Folder", "The folder where newly created extension files will be placed."));

                EditorGUILayout.SelectableLabel(extensionFolderPath, EditorStyles.textField, GUILayout.Height(16f));

                bool extDestChanged = false;
                if (GUILayout.Button(folderIcon, EditorStyles.miniButton, GUILayout.Width(32f), GUILayout.Height(16f)))
                {
                    GUI.SetNextControlName("");
                    GUI.FocusControl("");

                    string temp = EditorUtility.SaveFolderPanel("Select a folder within your project", extensionFolderPath, "");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        if (!temp.StartsWith(Application.dataPath))
                        {
                            extensionFolderPath = "Assets";
                        }
                        else
                        {
                            extensionFolderPath = "Assets" + temp.Replace(Application.dataPath, string.Empty);
                        }
                    }
                    extDestChanged = true;
                }
                EditorGUILayout.EndHorizontal();

                if (extDestChanged)
                {
                    EditorPrefs.SetString(ExtensionFolderKey, extensionFolderPath);
                }

                EditorGUI.indentLevel--;
            }

            wizardFoldout = EditorGUILayout.Foldout(wizardFoldout, new GUIContent(wizardFoldoutContent));
            if (wizardFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel(new GUIContent("Output Wizard","Configure a new output type by specifying a model and attributes."));
                if (GUILayout.Button("Open"))
                {
                    ShowOutputWizardwindow();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
        }

        protected void OnDestroy()
        { }

        private void ShowOutputWizardwindow()
        {
            EditorWindow.GetWindow(typeof(OutputWizardWindow));
        }

        [MenuItem(MENU_ITEM, false, 501)]
        private static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(CinemaFaceCapSettingsWindow));
        }
    }
}