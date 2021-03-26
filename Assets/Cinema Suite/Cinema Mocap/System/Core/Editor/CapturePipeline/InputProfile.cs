using CinemaSuite.CinemaMocap.System.Core.Capture;
using CinemaSuite.CinemaMocap.System.Core.Editor.UI;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using System;
using System.Collections.Generic;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline
{
    /// <summary>
    /// An attribute that should be applied to all derived InputProfiles.
    /// </summary>
    public class InputProfileAttribute : Attribute
    {
        private string profileName = "InputProfile";
        private int sensorCount = 1;
        private MocapWorkflow mocapPhase;

        public InputProfileAttribute(string profileName, MocapWorkflow mocapPhase, int sensorCount)
        {
            this.profileName = profileName;
            this.mocapPhase = mocapPhase;
            this.sensorCount = sensorCount;
        }

        public string ProfileName
        {
            get
            {
                return profileName;
            }
        }

        public MocapWorkflow MocapPhase
        {
            get
            {
                return mocapPhase;
            }
        }

        public int SensorCount
        {
            get
            {
                return sensorCount;
            }
        }
    }

    /// <summary>
    /// A delegate for handling when important input profile changes are made that subscribers need to be notified about.
    /// </summary>
    /// <param name="sender">the sender</param>
    /// <param name="args">event args</param>
    public delegate void InputProfileChangedHandler(object sender, EventArgs args);

    /// <summary>
    /// A delegate for handling live captured frames.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void FrameCaptureHandler(object sender, FrameDataEventArgs args);

    public delegate void FrameSelectedHandler(object sender, FrameSelectedEventArgs args);

    public class FrameDataEventArgs : EventArgs
    {
        public SkeletonFrameData SkeletonFrameData;

        public FrameDataEventArgs(SkeletonFrameData frameData)
        {
            this.SkeletonFrameData = frameData;
        }
    }

    public class FrameSelectedEventArgs : EventArgs
    {
        public MocapSession session;
        public int frame;

        public FrameSelectedEventArgs(MocapSession session, int frame)
        {
            this.session = session;
            this.frame = frame;
        }
    }

    public class InputProfileMetaData
    {
        public Type Type;
        public InputProfileAttribute Attribute;

        public InputProfileMetaData(Type t, InputProfileAttribute attribute)
        {
            this.Type = t;
            this.Attribute = attribute;
        }
    }

    public abstract class InputProfile
    {
        public event FrameCaptureHandler FrameCaptured;
        public event FrameSelectedHandler FrameSelected;
        public event InputProfileChangedHandler InputSkeletonTypeChanged;

        public float AspectRatio = 1f;

        public InputProfile() { }

        /// <summary>
        /// Trigger the FrameCaptured event.
        /// </summary>
        /// <param name="args">Arguments for the FrameCaptured event.</param>
        protected virtual void OnFrameCaptured(FrameDataEventArgs args)
        {
            if(FrameCaptured != null)
            {
                FrameCaptured(this, args);
            }
        }

        protected virtual void OnFrameSelected(FrameSelectedEventArgs args)
        {
            if(FrameSelected != null)
            {
                FrameSelected(this, args);
            }
        }

        protected virtual void OnInputSkeletonTypeChanged(EventArgs args)
        {
            if(InputSkeletonTypeChanged != null)
            {
                InputSkeletonTypeChanged(this, args);
            }
        }

        /// <summary>
        /// Unload any assets used by this mocap profile.
        /// </summary>
        public virtual void Destroy() { }

        /// <summary>
        /// Save the current field values
        /// </summary>
        public virtual void SaveEditorPrefs() { }

        /// <summary>
        /// Load any saved field values
        /// </summary>
        public virtual void LoadEditorPrefs() { }

        /// <summary>
        /// Update the mocap session profile.
        /// </summary>
        public abstract void Update();

        public abstract void DrawInputSettings();

        public abstract void DrawDisplayArea(CinemaMocapLayout layout);

        /// <summary>
        /// Turn on the devices associated with this mocap profile.
        /// </summary>
        /// <returns>True if successful.</returns>
        public abstract bool TurnOnDevice();

        /// <summary>
        /// Turn off the devices associated with this mocap profile.
        /// </summary>
        public abstract void TurnOffDevice();

        /// <summary>
        /// Load all input profiles context data found in the assembly.
        /// </summary>
        public static List<InputProfileMetaData> LoadMetaData(params MocapWorkflow[] filter)
        {
            List<InputProfileMetaData> metaData = new List<InputProfileMetaData>();

            List<Type> types = CinemaMocapHelper.GetInputProfiles();
            foreach (Type t in types)
            {
                foreach (InputProfileAttribute attribute in t.GetCustomAttributes(typeof(InputProfileAttribute), true))
                {
                    if (Array.Exists<MocapWorkflow>(filter, item => item == attribute.MocapPhase))
                    {
                        metaData.Add(new InputProfileMetaData(t, attribute));
                    }
                }
            }

            return metaData;
        }

        /// <summary>
        /// Returns true if the device is on.
        /// </summary>
        public abstract bool IsDeviceOn { get; }

        /// <summary>
        /// Should the Mocap Window show a section for hardware settings for this profile?
        /// </summary>
        public abstract bool ShowInputSettings { get; }

        public abstract InputSkeletonType InputSkeleton { get; }

        public abstract MocapSessionMetaData GetSessionMetaData();

        internal virtual MocapSession GetSession()
        {
            return null;
        }
    }
}