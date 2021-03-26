
using CinemaSuite.CinemaMocap.System.Core.Capture;
using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using BaseSystem = System;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline
{
    public class ReviewPipeline : MocapPipeline
    {
        private CaptureCache cache;
        private FrameSelectedEventArgs previousArgs;

        public ReviewPipeline(bool loadFromEditorPrefs) : base(loadFromEditorPrefs)
        {
            loadProfiles(loadFromEditorPrefs);
            MappingSettingsUpdated += ReviewPipeline_MappingSettingsUpdated;
            LoadEditorPrefs();

            spoolUpInputEvents();
        }

        private void ReviewPipeline_MappingSettingsUpdated(BaseSystem.Object sender, BaseSystem.EventArgs args)
        {
            if(previousArgs != null)
                InputProfile_FrameSelected(this, previousArgs);
        }

        protected override void loadProfiles(bool loadFromEditorPrefs)
        {
            base.loadProfiles(loadFromEditorPrefs, CinemaSuite.CinemaMocap.System.Core.MocapWorkflow.Review);
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

                InputProfile = BaseSystem.Activator.CreateInstance(inputProfiles[mocapProfileSelection].Type) as InputProfile;

                // Input Profile changed.
                inputProfileChanged();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            InputProfile.DrawInputSettings();
        }

        public override void DrawPipelineSettings()
        {
            EditorGUI.BeginDisabledGroup(!InputProfile.IsDeviceOn);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button(new GUIContent("Save"), EditorStyles.miniButton))
            {
                saveSession = false;

                MocapSession session = InputProfile.GetSession();
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

        private void InputProfile_FrameSelected(object sender, FrameSelectedEventArgs args)
        {
            previousArgs = args;
            if (MappingProfile != null && OutputProfile != null)
            {
                cache = new CaptureCache();

                // Build the cache
                int startingFrame = BaseSystem.Math.Max(args.frame - cache.CacheSize, 0);

                for (int i = startingFrame; i <= args.frame; i++)
                {
                    var elapsedTime = args.session.CaptureData[i].ElapsedMilliseconds;
                    var skeleton = NUICaptureHelper.GetNUISkeleton(args.session.CaptureData[i].Skeleton);

                    cache.AddNewFrame(skeleton, elapsedTime);

                    // Filter the raw input with enabled filters.
                    NUISkeleton filtered = skeleton;
                    foreach (MocapFilter filter in preMapFilters)
                    {
                        if (filter.Enabled)
                        {
                            filtered = filter.Filter(cache);
                            cache.AddFiltered(filter.Name, filtered);
                        }
                    }

                    // Convert the input skeleton to the normalized skeleton (Unity)
                    NUISkeleton mapped = MappingProfile.MapSkeleton(filtered);
                    cache.AddMapped(mapped);

                    // Apply any post-mapped filters selected by the user.
                    filtered = mapped;
                    foreach (MocapFilter filter in postMapFilters)
                    {
                        if (filter.Enabled)
                        {
                            filtered = filter.Filter(cache);
                            cache.AddFiltered(filter.Name, filtered);
                        }
                    }
                    cache.AddResult(filtered);
                }

                var s = NUICaptureHelper.GetNUISkeleton(args.session.CaptureData[args.frame].Skeleton);
                OutputProfile.UpdatePreview(cache.CurrentSkeleton, MappingProfile.GetHipPosition(s));
            }
        }
    }
}