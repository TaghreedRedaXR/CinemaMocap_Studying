using CinemaSuite.Core.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor
{
    public class OutputWizardWindow : EditorWindow
    {
        #region User Options
        private UnityEngine.GameObject asset;

        private string assetName = "";
        private string userFriendlyName = "";
        private string className = "";

        private string orientationNodePath = "";
        private string facePath = "";

        private List<BlendShapeOptions> blendShapes = new List<BlendShapeOptions>();
        #endregion

        #region File Options
        private string filePath = "Assets";
        #endregion

        #region UI
        private const string TITLE = "Output Wizard";
        private const string MENU_ITEM = "Window/Cinema Suite/Cinema Face Cap/Output Wizard";

        private ReorderableList reorderableList;
        private int controlId;

        private bool setupFoldout = true;
        private bool fileFoldout = true;
        private bool previewFoldout = true;

        private Vector2 mainScrollView;
        private Vector2 previewScrollView;

        private Texture2D miniThumbnail;
        private Texture2D assetPreview;
        private Texture2D folderIcon;

        static private bool showInvalidClassName = false;
        static private bool showAssetModelWarning = false;
        #endregion

        /// <summary>
        /// Called when the window is opened.
        /// Initializes all variables and sets up the system.
        /// </summary>
        void Awake()
        {
            // Setup window
#if UNITY_5 && !UNITY_5_0 || UNITY_2017_1_OR_NEWER
            base.titleContent = new GUIContent(TITLE);
#else
            base.title = TITLE;
#endif
            minSize = new Vector2(512, 256);
            miniThumbnail = AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
            folderIcon = EditorGUIUtility.Load("Cinema Suite/Cinema Face Cap/cs_Folder.png") as Texture2D;

            if(EditorPrefs.HasKey(CinemaFaceCapSettingsWindow.ExtensionFolderKey))
            {
                filePath = EditorPrefs.GetString(CinemaFaceCapSettingsWindow.ExtensionFolderKey);
            }
            else
            {
                filePath = CinemaFaceCapSettingsWindow.ExtensionFolderDefaultPath;
                EditorPrefs.SetString(CinemaFaceCapSettingsWindow.ExtensionFolderKey, CinemaFaceCapSettingsWindow.ExtensionFolderDefaultPath);
            }
        }

        /// <summary>
        /// Called when Window is enabled.
        /// </summary>
        void OnEnable()
        {
            populateFields(asset);

            reorderableList = new ReorderableList(blendShapes, typeof(BlendShapeOptions), true, true, true, true);

            var columns = new float[5] { 0.3f, 0.05f, 0.2f, 0.1f, 0.3f };

            reorderableList.drawHeaderCallback = (Rect rect) => {

                var rects = new Rect[5];
                float gap = 8f;
                for(int i = 0; i < rects.Length; i++)
                {
                    rects[i] = new Rect(rect);
                    float x = rect.x + 4;
                    if(i > 0)
                    {
                        x = rects[i - 1].x + rects[i - 1].width;
                    }
                    rects[i].x = x + gap;
                    rects[i].width = (rect.width-4-(gap * rects.Length)) * columns[i] + gap;
                }

                EditorGUI.LabelField(rects[0], "Blend Shape Name");
                EditorGUI.LabelField(rects[1], "Map");
                EditorGUI.LabelField(rects[2], "Input");
                EditorGUI.LabelField(rects[3], "Style");
                EditorGUI.LabelField(rects[4], "Function");
            };

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = reorderableList.list[index] as BlendShapeOptions;
                rect.y += 2;

                var rects = new Rect[5];
                float gap = 8f;
                for (int i = 0; i < rects.Length; i++)
                {
                    rects[i] = new Rect(rect);
                    float x = rect.x - gap;
                    if (i > 0)
                    {
                        x = rects[i - 1].x + rects[i - 1].width;
                    }
                    rects[i].x = x + gap;
                    rects[i].width = (rect.width - (gap * rects.Length)) * columns[i] + gap;
                    rects[i].height = EditorGUIUtility.singleLineHeight;

                }

                element.name = EditorGUI.TextField(rects[0], element.name);
                if (!element.isMapped)
                {
                    element.isMapped = EditorGUI.Toggle(rects[1], element.isMapped);
                }
                else
                {
                    element.isMapped = EditorGUI.Toggle(rects[1], element.isMapped);
                    element.hint = (FaceShapeAnimations) EditorGUI.EnumPopup(rects[2], element.hint);
                    var temp  = (MappingStyle) EditorGUI.EnumPopup(rects[3], element.mappingStyle);
                    if(temp != element.mappingStyle)
                    {
                        element.mappingStyle = temp;
                        element.function = getFunctionFromStyle(element.mappingStyle);
                    }
                    EditorGUI.BeginDisabledGroup(element.mappingStyle != MappingStyle.Custom);
                    rects[4].width -= 2;
                    element.function = EditorGUI.TextField(rects[4], element.function);
                    EditorGUI.EndDisabledGroup();
                }
            };
        }

        /// <summary>
        /// Called when the window needs to be redrawn or an event has been triggered.
        /// </summary>
        void OnGUI()
        {
            mainScrollView = EditorGUILayout.BeginScrollView(mainScrollView);

            // Draw setup area
            setupFoldout = EditorGUILayout.Foldout(setupFoldout, "Setup");
            EditorGUI.indentLevel++;
            if (setupFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Asset");

                int controlId = GUIUtility.GetControlID(FocusType.Passive);
                if (GUILayout.Button(new GUIContent(asset == null ? "None" : asset.name, miniThumbnail, "The character model"), EditorStyles.objectField, GUILayout.MaxHeight(16)))
                {
                    EditorGUIUtility.ShowObjectPicker<UnityEngine.GameObject>(asset, false, "t:Model", controlId);
                }

                if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorClosed")
                {
                    if (EditorGUIUtility.GetObjectPickerControlID() == controlId)
                    {
                        showAssetModelWarning = false;
                        var tempAsset = EditorGUIUtility.GetObjectPickerObject();
                        if (tempAsset != asset)
                        {
                            string path = AssetDatabase.GetAssetPath(tempAsset);
                            var split = path.Split('/');

                            if (split[0].Equals("Assets") && split.Contains("Resources"))
                            {
                                populateFields(tempAsset);
                                asset = tempAsset as GameObject;
                            }
                            else
                            {
                                showAssetModelWarning = true;
                            }
                        }

                        Event.current.Use();
                        Repaint();
                    }
                }
                EditorGUILayout.EndHorizontal();

                assetName = EditorGUILayout.TextField(new GUIContent("Asset Name", "The name of the model."), assetName);
                className = EditorGUILayout.TextField(new GUIContent("Class Name", "The unique class name of this face."), className);
                userFriendlyName = EditorGUILayout.TextField(new GUIContent("User Friendly Name", "The name that will appear in UI elements."), userFriendlyName);

                orientationNodePath = EditorGUILayout.TextField(new GUIContent("Orientation Node Path", "The path to the orientation node. Delimited by period '.'"), orientationNodePath);
                facePath = EditorGUILayout.TextField(new GUIContent("Face Path", "The path to the face containing the blend shapes."), facePath);

                // Draw reorderable list
                EditorGUI.indentLevel--;
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(reorderableList.GetHeight()));
                rect.x += 16f; rect.width -= 16f;
                reorderableList.DoList(rect);
                EditorGUI.indentLevel++;

                
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Draw File foldout section
            fileFoldout = EditorGUILayout.Foldout(fileFoldout, "File");
            EditorGUI.indentLevel++;
            if (fileFoldout)
            {
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Folder", "The folder the newly created file will be placed."));

                EditorGUILayout.SelectableLabel(filePath, EditorStyles.textField, GUILayout.Height(16f));

                bool extDestChanged = false;
                if (GUILayout.Button(folderIcon, EditorStyles.miniButton, GUILayout.Width(32f), GUILayout.Height(16f)))
                {
                    // Ensure the path text field does not have focus, or else we cannot change the contents.
                    GUI.SetNextControlName("");
                    GUI.FocusControl("");

                    string temp = EditorUtility.SaveFolderPanel("Select a folder within your project", filePath, "");

                    // Pressing cancel returns an empty string, don't clear the previous text on cancel.
                    if (!string.IsNullOrEmpty(temp))
                    {
                        if (!temp.StartsWith(Application.dataPath))
                        {
                            EditorGUILayout.HelpBox("Invalid selection!\nYou must select a location within your project's \"Assets\" folder.", MessageType.Warning);
                            filePath = "Assets";
                        }
                        else
                        {
                            filePath = "Assets" + temp.Replace(Application.dataPath, string.Empty);
                        }
                    }
                    extDestChanged = true;
                }
                EditorGUILayout.EndHorizontal();

                

                if (!filePath.Contains("Editor"))
                {
                    EditorGUILayout.HelpBox("The file must be placed within an \"Editor\" folder.", MessageType.Warning);
                }
                else if (extDestChanged)
                {
                    EditorPrefs.SetString(CinemaFaceCapSettingsWindow.ExtensionFolderKey, filePath);
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(" ");
                if (GUILayout.Button("Create Output Profile"))
                {
                    className = GenerateClassName(className);
                    createFile(getOutputTemplate());
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Draw Preview Area
            previewFoldout = EditorGUILayout.Foldout(previewFoldout, "Preview");
            EditorGUI.indentLevel++;
            if (previewFoldout)
            {
                previewScrollView = EditorGUILayout.BeginScrollView(previewScrollView, GUILayout.MinHeight(256), GUILayout.ExpandHeight(true));
                EditorGUILayout.TextArea(getOutputTemplate());
                EditorGUILayout.EndScrollView();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.EndScrollView();

            if (showInvalidClassName)
            {
            EditorUtility.DisplayDialog("Renamed Class Name", "The class name entered contains invalid characters.\n" +
                    "It must contain only letters, numbers or underscores and start with either a letter or an underscore." +
                    "\n\nThe class name is saved as:\n " + className, "OK");
            showInvalidClassName = false;
            }

            if (showAssetModelWarning)
                EditorGUILayout.HelpBox("The asset model must be placed in a Resources folder. Your Asset selection has been ignored.", MessageType.Warning);
        }

        private void populateFields(UnityEngine.Object tempAsset)
        {
            if (tempAsset != null)
            {
                miniThumbnail = AssetPreview.GetMiniThumbnail(tempAsset);
                assetName = tempAsset.name;
                className = tempAsset.name;
                userFriendlyName = tempAsset.name;

                var transform = (tempAsset as GameObject).transform;

                // Get the path to the orientation node. Neck > Head
                string orientationPath = "";
                var paths = getChildrenPaths(transform);
                double orientationDice = 0;
                foreach(var path in paths)
                {
                    var split = path.Split('.',':');
                    double neckDice = "Neck".DiceCoefficient(split[split.Length - 1]);
                    if(neckDice > 0.6 && neckDice > orientationDice)
                    {
                        orientationDice = neckDice;
                        orientationPath = path;
                    }
                }

                orientationDice = 0;
                if (string.IsNullOrEmpty(orientationPath))
                {
                    foreach (var path in paths)
                    {
                        var split = path.Split('.',':');
                        double headDice = "Head".DiceCoefficient(split[split.Length - 1]);
                        if (headDice > 0.6 && headDice > orientationDice)
                        {
                            orientationDice = headDice;
                            orientationPath = path;
                        }
                    }
                }

                orientationNodePath = orientationPath;
                if(!string.IsNullOrEmpty(orientationNodePath) && orientationNodePath.Substring(0, transform.name.Length) == transform.name)
                {
                    orientationNodePath = orientationNodePath.Substring(transform.name.Length + 1);
                }
                
                // Get the path to the blend shape object.
                facePath = getBlendShapeObjectPath(transform);
                facePath = facePath.Substring(transform.name.Length + 1);

                var children = facePath.Split('.');

                Transform temp = transform;
                foreach (var child in children)
                {
                    temp = temp.Find(child);
                }
                var skinnedMeshRenderer = temp.GetComponent<SkinnedMeshRenderer>();
                blendShapes.Clear();

                for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    var bso = new BlendShapeOptions();
                    bso.index = i;
                    bso.name = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
                    bso.mappingStyle = MappingStyle.Standard;
                    bso.function = getFunctionFromStyle(bso.mappingStyle);

                    var names = Enum.GetNames(typeof(FaceShapeAnimations));

                    int index = -1;
                    double diceCoefficient = -1f;
                    for(int j = 0; j < names.Length; j++)
                    {
                        var split = bso.name.ToUpper().Split('.');
                        double tempDice = names[j].ToUpper().DiceCoefficient(split[split.Length-1]);
                        
                        if (diceCoefficient < tempDice)
                        {
                            diceCoefficient = tempDice;
                            index = j;
                        }
                    }

                    if(index >= 0 && diceCoefficient >= 0.6)
                    {
                        bso.isMapped = true;
                        bso.hint = (FaceShapeAnimations) index;
                    }

                    blendShapes.Add(bso);
                }
            }
            else
            {
                resetFields();
            }
        }

        private List<string> getChildrenPaths(Transform transform)
        {
            var retVal = new List<string>() { transform.name };

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var currentChild = transform.GetChild(i);
                var childPaths = getChildrenPaths(currentChild);
                for(int j = 0; j < childPaths.Count; j++)
                {
                    childPaths[j] = string.Format("{0}.{1}", transform.name, childPaths[j]);
                }
                retVal.AddRange(childPaths);
            }

            return retVal;
        }

        private void resetFields()
        {
            miniThumbnail = AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
            assetName = string.Empty;
            className = string.Empty;
            userFriendlyName = string.Empty;

            orientationNodePath = string.Empty;
            facePath = string.Empty;

            blendShapes.Clear();
        }

        private string getBlendShapeObjectPath(Transform transform)
        {
            string retVal = string.Empty;
            var component = transform.GetComponent<SkinnedMeshRenderer>();
            if (component != null)
            {
                if (component.sharedMesh.blendShapeCount > 0)
                {
                    return component.name;
                }
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var currentChild = transform.GetChild(i);
                var childPath = getBlendShapeObjectPath(currentChild);
                if (!string.IsNullOrEmpty(childPath))
                {
                    retVal = transform.name + "." + childPath;
                }
            }

            return retVal;
        }

        private string getFunctionFromStyle(MappingStyle mappingStyle)
        {
            string retVal = "";
            if(mappingStyle == MappingStyle.Standard)
            {
                retVal = "x => x * 100";
            }
            else if(mappingStyle == MappingStyle.Positive)
            {
                retVal = "x => Math.Max(x, 0f) * 100f";
            }
            else if (mappingStyle == MappingStyle.Negative)
            {
                retVal = "x => Math.Min(x, 0f) * -100f";
            }
            else if (mappingStyle == MappingStyle.Custom)
            {
                retVal = "x => x";
            }
            return retVal;
        }

        private void createFile(string template)
        {
            if(string.IsNullOrEmpty(className))
            {
                Debug.LogWarning("Class name is required to create a new output profile.");
                return;
            }

            if(!AssetDatabase.IsValidFolder(filePath))
            {
                Debug.Log("Folder path provided did not exist. We made it for you.");
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

            var path = string.Format("{0}/{1}.cs", filePath, className);

            if (System.IO.File.Exists(path))
            {
                if (!EditorUtility.DisplayDialog(className + ".cs exists", "Would you like to overwrite file?", "Yes", "No"))
                {
                    showInvalidClassName = false;
                    return;
                }
            }

            System.IO.File.WriteAllText(path, template);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Get the final output to create a .cs file
        /// </summary>
        /// <returns></returns>
        private string getOutputTemplate()
        {
            var fileContent =

            "using System;\n" +
            "using CinemaSuite.CinemaFaceCap.App.Core;\n" +
            "using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;\n" +
            "using CinemaSuite.CinemaFaceCap.App.Core.Mapping;\n" +
            "\n" +
            "namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline.Output \n" +
            "{\n" +
            "\t[Name(\"" + userFriendlyName + "\")]\n" +
            "\tpublic class " + className + " : StandardOutputFace \n" +
            "\t{\n" +

            "\t\tpublic override FaceStructure GetTargetStructure()\n" +
            "\t\t{\n" +

            "\t\t\tvar faceStructure = new FaceStructure(\"" + assetName + "\");\n" +

            "\t\t\tfaceStructure.OrientationNodePath = \"" + orientationNodePath + "\";\n" +
            "\t\t\tfaceStructure.FacePath = \"" + facePath + "\";\n";

            foreach (var blendShape in blendShapes)
            {
                if (blendShape.isMapped)
                {
                    fileContent += "\t\t\tfaceStructure.Add(\"" + blendShape.name + "\", FaceShapeAnimations." + blendShape.hint.ToString() + ", " + blendShape.function + ");\n";
                }
            }

            fileContent +=
            "\t\t\treturn faceStructure;\n" +

            "\t\t}\n" +
            "\t}\n" +
            "}";

            return fileContent;
        }

        [MenuItem(MENU_ITEM, false, 500)]
        private static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(OutputWizardWindow));
        }

        private enum MappingStyle
        {
            Standard,
            Positive,
            Negative,
            Custom
        }

        private class BlendShapeOptions
        {
            public int index;
            public string name;

            public bool isMapped = false;
            public FaceShapeAnimations hint;
            public MappingStyle mappingStyle;
            public string function;
        }

        private static string GenerateClassName(string value)
        {
            string oldClassName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
            string newClassName = "";
            bool isValid = Microsoft.CSharp.CSharpCodeProvider.CreateProvider("C#").IsValidIdentifier(oldClassName);
            
            if (!isValid)
            { 
                // File name contains invalid chars, remove them
                Regex regex = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");
                newClassName = regex.Replace(oldClassName, "");
                
                // Class name doesn't begin with a letter, insert an underscore
                if (!char.IsLetter(newClassName, 0))
                {
                    newClassName = newClassName.Insert(0, "_");
                }
                showInvalidClassName = true;
                return newClassName.Replace(" ", string.Empty);
            }
            return oldClassName;
        }
    }
}
