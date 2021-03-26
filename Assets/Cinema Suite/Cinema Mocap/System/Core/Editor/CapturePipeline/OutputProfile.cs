using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline
{

    public class OutputProfileAttribute : Attribute
    {
        private string name = "Standard 20 Joint";

        public OutputProfileAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }
    }


    /// <summary>
    /// A Profile for loading rig data and saving animations.
    /// </summary>
    public abstract class OutputProfile
    {
        /// <summary>
        /// Initialize the OutputProfile and load rig data and prefabs.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Save an animation based on the mocap data.
        /// </summary>
        /// <param name="animation">The animation captured.</param>
        public abstract void SaveAnimation(NUIHumanoidAnimation animation);

        /// <summary>
        /// Create a preview model in the Scene View.
        /// </summary>
        public abstract void CreatePreview();

        /// <summary>
        /// Update the Preview Model.
        /// </summary>
        /// <param name="skeleton">The skeleton to update the preview with.</param>
        public abstract void UpdatePreview(NUISkeleton skeleton, Vector3 position);

        /// <summary>
        /// Reset the preview model back to it's initial pose.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Destroy the output profile.
        /// </summary>
        public abstract void Destroy();

        /// <summary>
        /// Draw any custom output settings to the UI.
        /// </summary>
        public abstract void DrawOutputSettings();

        /// <summary>
        /// Save the current field values
        /// </summary>
        public virtual void SaveEditorPrefs() { }

        /// <summary>
        /// Load any saved field values
        /// </summary>
        public virtual void LoadEditorPrefs() { }

        /// <summary>
        /// Get the target structure of the skeleton, to be given to the mapper.
        /// </summary>
        /// <returns>A target skeleton structure, in Unity friendly space.</returns>
        public abstract NUISkeleton GetTargetStructure();

        /// <summary>
        /// Load all output profiles context data found in the assembly.
        /// </summary>
        public static List<TypeLabelContextData> LoadMetaData()
        {
            List<TypeLabelContextData> outputProfiles = new List<TypeLabelContextData>();

            List<Type> types = CinemaMocapHelper.GetOutputProfiles();
            foreach (Type t in types)
            {
                foreach (OutputProfileAttribute attribute in t.GetCustomAttributes(typeof(OutputProfileAttribute), true))
                {
                    outputProfiles.Add(new TypeLabelContextData(t, attribute.Name));
                }
            }

            return outputProfiles;
        }

        public static List<TypeLabelContextData> LoadMetaData(List<Type> outputTypes)
        {
            var outputProfiles = new List<TypeLabelContextData>();

            List<Type> types = CinemaMocapHelper.GetOutputProfiles();
            foreach (Type t in types)
            {
                if (outputTypes.Contains(t))
                {
                    foreach (OutputProfileAttribute attribute in t.GetCustomAttributes(typeof(OutputProfileAttribute), true))
                    {
                        outputProfiles.Add(new TypeLabelContextData(t, attribute.Name));
                    }
                }
            }

            return outputProfiles;
        }
    }
}