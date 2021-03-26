using CinemaSuite.CinemaMocap.System.Behaviours;
using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Capture;
using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using System;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline.Input
{
    [InputProfileAttribute("Playback", MocapWorkflow.Review, 1)]
    public class SessionPlaybackInputProfile : InputProfile
    {
        private GameObject skeletonRendererInstance;
        private const string SKELETON_RENDERER_NAME = "SkeletonRenderer";
        
        MocapSession session;
        int frame = 0;

        string sessionAssetPath;

        //EditorPrefs
        private const string PLAYBACK_INPUT_SESSION_ASSET_PATH_KEY = "CinemaSuite.PlaybackSessionAssetPath";

        public SessionPlaybackInputProfile()
        {
            LoadEditorPrefs();
        }

        /// <summary>
        /// Does this input profile have input settings?
        /// </summary>
        public override bool ShowInputSettings
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Draw the Input settings for session playback.
        /// </summary>
        public override void DrawInputSettings()
        {
            MocapSession temp = EditorGUILayout.ObjectField(new GUIContent("Session"), session, typeof(MocapSession), false) as MocapSession;
            sessionAssetPath = AssetDatabase.GetAssetPath(temp);

            if (temp != session)
            {
                session = temp;
                base.OnInputSkeletonTypeChanged(new EventArgs());
                EditorPrefs.SetString(PLAYBACK_INPUT_SESSION_ASSET_PATH_KEY, sessionAssetPath);
            }

            EditorGUI.BeginDisabledGroup(session == null);

            int lastFrame = session == null ? 0 : session.CaptureData.Count - 1;

            EditorGUILayout.BeginHorizontal();

            int tempFrame = EditorGUILayout.IntField("Frame", frame);


            if (GUILayout.Button("<", EditorStyles.miniButton, GUILayout.Width(25f)))
            {
                tempFrame--;
            }

            if (GUILayout.Button(">", EditorStyles.miniButton, GUILayout.Width(25f)))
            {
                tempFrame++;
            }

            //bounds
            if (tempFrame < 0) tempFrame = 0;
            if (tempFrame > lastFrame) tempFrame = lastFrame;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");

            // "Start rendering" button
            if (skeletonRendererInstance == null)
            {
                if (GUILayout.Button(new GUIContent("Render Raw Skeleton"), EditorStyles.miniButton))
                {
                    skeletonRendererInstance = GameObject.Find(SKELETON_RENDERER_NAME);

                    if (skeletonRendererInstance == null)
                    {
                        skeletonRendererInstance = new GameObject(SKELETON_RENDERER_NAME);
                        skeletonRendererInstance.AddComponent<SkeletonRenderer>();
                    }

                    skeletonRendererInstance.GetComponent<SkeletonRenderer>().UpdateSkeleton(session.CaptureData[frame].Skeleton, this.InputSkeleton);
                }
            }
            else // "Stop rendering" button
            {
                Color tempColor = GUI.color;
                GUI.color = Color.magenta;
                if (GUILayout.Button(new GUIContent("Stop Rendering"), EditorStyles.miniButton))
                {
                    skeletonRendererInstance = null;
                    UnityEngine.Object.DestroyImmediate(GameObject.Find(SKELETON_RENDERER_NAME));
                }
                GUI.color = tempColor;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            if (session != null && tempFrame != frame)
            {
                frame = tempFrame;

                var data = new FrameSelectedEventArgs(session, frame);
                OnFrameSelected(data);

                skeletonRendererInstance = GameObject.Find(SKELETON_RENDERER_NAME);
                if (skeletonRendererInstance != null)
                {
                    skeletonRendererInstance.GetComponent<SkeletonRenderer>().UpdateSkeleton(session.CaptureData[frame].Skeleton, this.InputSkeleton);
                }
            }

            SaveEditorPrefs();
        }

        public override void DrawDisplayArea(CinemaMocapLayout layout)
        {
            // Eventually we can create session playback viewers.
            // For example: Timeline scrubber with curve editing.

            GUIStyle style = new GUIStyle("helpBox");
            style.fontSize = 20;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Box(layout.Area, "<Timeline Reviewer coming soon.>", style);
        }

        // Maybe
        public override bool IsDeviceOn
        {
            get
            {
                return (session != null);
            }
        }

        public override void TurnOffDevice()
        {
            throw new NotImplementedException();
        }

        public override bool TurnOnDevice()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            // Left blank
        }

        public override void Destroy()
        {
            SaveEditorPrefs();
            GameObject.DestroyImmediate(skeletonRendererInstance);
            base.Destroy();

        }

        public override void SaveEditorPrefs()
        {
            EditorPrefs.SetString(PLAYBACK_INPUT_SESSION_ASSET_PATH_KEY, sessionAssetPath);
        }

        public override void LoadEditorPrefs()
        {
            if (EditorPrefs.HasKey(PLAYBACK_INPUT_SESSION_ASSET_PATH_KEY))
            {
                sessionAssetPath = EditorPrefs.GetString(PLAYBACK_INPUT_SESSION_ASSET_PATH_KEY);

                session = (MocapSession)AssetDatabase.LoadAssetAtPath(sessionAssetPath, typeof(MocapSession));
            }
        }

        public override InputSkeletonType InputSkeleton
        {
            get
            {
                if (session == null || session.MetaData == null)
                {
                    return InputSkeletonType.None;
                }
                else
                {
                    return session.MetaData.InputSkeletonType;
                }
            }
        }

        internal override MocapSession GetSession()
        {
            return session;
        }

        public override MocapSessionMetaData GetSessionMetaData()
        {
            if (session != null)
            {
                return session.MetaData;
            }
            return null;
        }

    }
}
