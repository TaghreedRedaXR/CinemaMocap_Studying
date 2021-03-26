#if UNITY_5 || UNITY_2017_1_OR_NEWER
using BaseSystem = System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using CinemaSuite.Utility;


/// <summary>
/// Settings window for Cinema Mocap.
/// </summary>
public class AnimationCombinerWindow : EditorWindow
{

#region Language
    private const string TITLE = "Combiner";
    private const string MENU_ITEM = "Window/Cinema Suite/Generate Face and Body Controller";
    private const string FILE_EXTENSION = "controller";

#endregion

    private GameObject model;
    private AnimationClip faceClip;
    private AnimationClip bodyClip;
    private string controllerName = "";
    private bool addControllerToModel = true;

    private AvatarMask faceMask;
    private AvatarMask bodyMask;


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

        faceMask = Resources.Load("FaceAvatarMask") as AvatarMask;
        bodyMask = Resources.Load("BodyAvatarMask") as AvatarMask;
    }

    /// <summary>
    /// Show the Cinema Mocap Window
    /// </summary>
    [MenuItem(MENU_ITEM, false, 50)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AnimationCombinerWindow));
    }



    /// <summary>
    /// Draw the Window's contents
    /// </summary>
    protected void OnGUI()
    {
        string advice =
            "This tool will help create a starting animation controller that uses both face and body " +
            "animations. Always make sure your model's animation type is set to \"Humanoid\". This " +
            "setting can be found by clicking on the prefab in the Project Hierarchy, and clicking on " + 
            "the \"Rig\" tab.";

        EditorGUILayout.HelpBox(advice, MessageType.None);

        GameObject tempGO = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Model"), model, typeof(GameObject), true);
        if (tempGO != model)
            model = tempGO;

        AnimationClip tempAnimFace = (AnimationClip)EditorGUILayout.ObjectField(new GUIContent("Face Animation"), faceClip, typeof(AnimationClip), false);
        if (tempAnimFace != faceClip)
            faceClip = tempAnimFace;

        AnimationClip tempAnimBody = (AnimationClip)EditorGUILayout.ObjectField(new GUIContent("Body Animation"), bodyClip, typeof(AnimationClip), false);
        if (tempAnimBody != bodyClip)
            bodyClip = tempAnimBody;

        string tempControllerName = EditorGUILayout.TextField(new GUIContent("Controller Name"), controllerName);
        if (tempControllerName != controllerName)
            controllerName = tempControllerName;

        bool tempAddController = EditorGUILayout.ToggleLeft(new GUIContent("Automatically add controller to model."), addControllerToModel);
        if (tempAddController != addControllerToModel)
            addControllerToModel = tempAddController;

        EditorGUI.BeginDisabledGroup(!IsInputValid());

        if (GUILayout.Button(new GUIContent("Create Combined Animator Controller")))
        {
            CreateCombinedAnimatorController();
        }

        EditorGUI.EndDisabledGroup();
    }

    private bool IsInputValid()
    {
        if (model == null) return false;
        if (faceClip == null) return false;
        if (bodyClip == null) return false;
        if (controllerName.Length == 0) return false;

        return true;
    }

    private void CreateCombinedAnimatorController()
    {
        //Create a new controller
        AnimatorController ac = CreateAnimationControllerAsset();

        //Optionally, add it to the model.

        if (addControllerToModel)
        {
            Animator animator = model.GetComponent<Animator>();
            if (!animator)
            {
                animator = model.AddComponent<Animator>();
            }        
            animator.runtimeAnimatorController = ac;
        }

    }

    private AnimatorController CreateAnimationControllerAsset()
    {
        string fileName = controllerName;
        string newFileName = fileName;
        if (File.Exists(string.Format("{0}/{1}.{2}", Application.dataPath, newFileName, FILE_EXTENSION)))
        {
            newFileName = Files.GetUniqueFilename(Application.dataPath, fileName, FILE_EXTENSION);
            Debug.LogWarning(string.Format(Files.ERROR_FORMAT_DUPLICATE_FILE_NAME, fileName, newFileName, FILE_EXTENSION));
        }

        // Creates the controller
        var controller = AnimatorController.CreateAnimatorControllerAtPath(string.Format("Assets/{0}.{1}", newFileName, FILE_EXTENSION));
        controller.RemoveLayer(0); //remove the default layer

        // Add Layers and state machines
        AnimatorControllerLayer faceLayer = new AnimatorControllerLayer();
        AnimatorControllerLayer bodyLayer = new AnimatorControllerLayer();
        faceLayer.stateMachine = new AnimatorStateMachine();
        bodyLayer.stateMachine = new AnimatorStateMachine();

        faceLayer.name = "Face Layer";
        faceLayer.defaultWeight = 1;
        faceLayer.avatarMask = faceMask;
        faceLayer.blendingMode = AnimatorLayerBlendingMode.Override;

        bodyLayer.name = "Body Layer";
        bodyLayer.defaultWeight = 1;
        bodyLayer.avatarMask = bodyMask;
        bodyLayer.blendingMode = AnimatorLayerBlendingMode.Override;

        controller.AddLayer(faceLayer);
        controller.AddLayer(bodyLayer);

        var faceStateMachine = faceLayer.stateMachine;
        var bodyStateMachine = bodyLayer.stateMachine;

        // Add States
        var faceAnimState = faceStateMachine.AddState("Face Animation");
        var bodyAnimState = bodyStateMachine.AddState("Body Animation");

        // Add animation to states
        faceAnimState.motion = faceClip;
        bodyAnimState.motion = bodyClip;

        return controller;
    }
}
#endif