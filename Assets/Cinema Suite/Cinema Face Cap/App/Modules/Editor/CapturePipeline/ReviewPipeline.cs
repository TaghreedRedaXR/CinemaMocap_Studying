
using UnityEditor;
using UnityEngine;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaFaceCap.App.Core;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using CinemaSuite.CinemaFaceCap.App.Core.Capture;
using System;

namespace CinemaSuite.CinemaFaceCap.App.Modules.Editor.CapturePipeline
{
    public class ReviewPipeline : FaceCapPipeline
    {
        private CaptureCache cache;
        private FrameSelectedEventArgs previousArgs;

        public ReviewPipeline(bool loadFromEditorPrefs) : base(loadFromEditorPrefs)
        {
            MappingSettingsUpdated += ReviewPipeline_MappingSettingsUpdated;
        }

        private void ReviewPipeline_MappingSettingsUpdated(System.Object sender, System.EventArgs args)
        {
            if (previousArgs != null)
            {
                buildCache();
                InputProfile_FrameSelected(this, previousArgs);
            }
        }

        protected override void loadProfiles(bool loadFromEditorPrefs)
        {
            base.loadProfiles(loadFromEditorPrefs, Workflow.Review);
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

                InputProfile = System.Activator.CreateInstance(inputProfiles[mocapProfileSelection].Type) as InputProfile;

                // Input Profile changed.
                inputProfileChanged();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            InputProfile.DrawInputSettings();
        }

        public override void DrawPipelineSettings()
        {
            EditorGUI.BeginDisabledGroup(InputProfile.InputFace == InputFace.None);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button(new GUIContent("Save"), EditorStyles.miniButton))
            {
                saveSession = false;

                var session = InputProfile.GetSession();
                saveAnimation(session);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        protected override void spoolUpInputEvents()
        {
            base.spoolUpInputEvents();
            InputProfile.FrameSelected += InputProfile_FrameSelected;
        }

        private MappedFaceCapFrame getFaceCapFrame(FaceCapFrameData faceCapFrameData)
        {
            MappedFaceCapFrame result = new MappedFaceCapFrame();
            result.rotation = faceCapFrameData.FaceOrientation;

            for (int i = 0; i < faceCapFrameData.AnimationUnits.Count; i++)
            {
                result.AnimationUnits.Add((FaceShapeAnimations)i, faceCapFrameData.AnimationUnits[i]);
            }

            return result;
        }

        private void InputProfile_FrameSelected(object sender, FrameSelectedEventArgs args)
        {
            previousArgs = args;

            if (MappingProfile != null && OutputProfile != null)
            {
                if (cache == null)
                {
                    buildCache();
                }
                OutputProfile.UpdatePreview(cache.GetResult(args.frame));
            }
        }

        protected override void inputProfileChanged()
        {
            base.inputProfileChanged();
            buildCache();
        }

        private void buildCache()
        {
            if (previousArgs == null || previousArgs.session == null) return;
            cache = new CaptureCache();
            var session = previousArgs.session;
            int frameCount = session.CaptureData.Count;
            cache.CacheSize = frameCount;
            
            for (int i = 0; i < frameCount; i++)
            {
                var elapsedTime = session.CaptureData[i].ElapsedMilliseconds;
                var face = getFaceCapFrame(session.CaptureData[i].FrameData);

                cache.AddNewFrame(face, elapsedTime);

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
                cache.AddResult(filtered);
            }
        }
    }
}