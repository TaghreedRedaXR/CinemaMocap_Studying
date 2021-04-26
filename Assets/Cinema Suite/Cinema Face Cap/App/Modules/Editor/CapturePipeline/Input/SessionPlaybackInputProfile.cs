
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.UI;
using System;
using CinemaSuite.CinemaFaceCap.App.Core.Capture;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline.Input
{
    [InputProfileAttribute("Playback", Workflow.Review, 1)]
    public class SessionPlaybackInputProfile : InputProfile
    {
        FaceCapSession session;
        int frame = 0;

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
            var temp = EditorGUILayout.ObjectField(new GUIContent("Session"), session, typeof(FaceCapSession), false) as FaceCapSession;
            if (temp != session)
            {
                session = temp;
                base.OnInputTypeChanged(new EventArgs());
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
            EditorGUI.EndDisabledGroup();

            if (session != null && tempFrame != frame)
            {
                frame = tempFrame;

                var data = new FrameSelectedEventArgs(session, frame);
                OnFrameSelected(data);
            }
        }

        public override void DrawDisplayArea(CinemaFaceCapLayout layout)
        {
            GUIStyle style = new GUIStyle("helpBox");
            style.fontSize = 20;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Box(layout.Area, "<Timeline Reviewer coming soon.>", style);
        }

        public override bool IsDeviceOn
        {
            get
            {
                return false;
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

        public override InputFace InputFace
        {
            get
            {
                if(session == null || session.MetaData == null)
                {
                    return InputFace.None;
                }
                else
                {
                    return InputFace.SeventeenAnimationUnits;
                }
            }
        }

        internal override FaceCapSession GetSession()
        {           
            return session;
        }

        public override FaceCapSessionMetaData GetSessionMetaData()
        {
            if(session != null)
            {
                return session.MetaData;
            }
            return null;
        }        
    }
}
