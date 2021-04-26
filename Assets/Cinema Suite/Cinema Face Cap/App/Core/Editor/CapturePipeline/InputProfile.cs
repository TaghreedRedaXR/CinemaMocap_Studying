
using CinemaSuite.CinemaFaceCap.App.Core.Capture;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.UI;
using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline
{
    /// <summary>
    /// An attribute that should be applied to all derived InputProfiles.
    /// </summary>
    public class InputProfileAttribute : Attribute
    {
        private string profileName = "InputProfile";
        private int sensorCount = 1;
        private Workflow mocapPhase;

        public InputProfileAttribute(string profileName, Workflow mocapPhase, int sensorCount)
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

        public Workflow MocapPhase
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
        public FaceCapFrameData FaceCapFrameData;

        public FrameDataEventArgs(FaceCapFrameData frameData)
        {
            this.FaceCapFrameData = frameData;
        }
    }

    public class FrameSelectedEventArgs : EventArgs
    {
        public FaceCapSession session;
        public int frame;

        public FrameSelectedEventArgs(FaceCapSession session, int frame)
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
        public event InputProfileChangedHandler InputTypeChanged;

        public float AspectRatio = 1f;

        public InputProfile() { }

        /// <summary>
        /// Trigger the FrameCaptured event.
        /// </summary>
        /// <param name="args">Arguments for the FrameCaptured event.</param>
        protected virtual void OnFrameCaptured(FrameDataEventArgs args)
        {
            if (FrameCaptured != null)
            {
                FrameCaptured(this, args);
            }
        }

        protected virtual void OnFrameSelected(FrameSelectedEventArgs args)
        {
            if (FrameSelected != null)
            {
                FrameSelected(this, args);
            }
        }

        protected virtual void OnInputTypeChanged(EventArgs args)
        {
            if (InputTypeChanged != null)
            {
                InputTypeChanged(this, args);
            }
        }

        /// <summary>
        /// Unload any assets used by this profile.
        /// </summary>
        public virtual void Destroy() { }

        /// <summary>
        /// Update the mocap session profile.
        /// </summary>
        public abstract void Update();

        public abstract void DrawInputSettings();

        public abstract void DrawDisplayArea(CinemaFaceCapLayout layout);

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
        public static List<InputProfileMetaData> LoadMetaData(params Workflow[] filter)
        {
            var metaData = new List<InputProfileMetaData>();

            List<Type> types = CinemaFaceCap.App.Core.Editor.Utility.Helper.GetInputProfiles();
            foreach (Type t in types)
            {
                foreach (InputProfileAttribute attribute in t.GetCustomAttributes(typeof(InputProfileAttribute), true))
                {
                    if (Array.Exists<Workflow>(filter, item => item == attribute.MocapPhase))
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

        public abstract InputFace InputFace { get; }

        public abstract FaceCapSessionMetaData GetSessionMetaData();

        internal virtual FaceCapSession GetSession()
        {
            return null;
        }
    }
}