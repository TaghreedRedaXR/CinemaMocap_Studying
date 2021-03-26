// Based on the double exponential filter from Microsoft's XNA Avateering demo.
using CinemaSuite.CinemaMocap.System.Core;
using CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Modules.Editor.CapturePipeline.Filters
{
    [NameAttribute("General Smoothing")]
    [MocapFilterAttribute(true)]
    [Ordinal(1)]
    public class SkeletonJointsPositionDoubleExponentialFilter : MocapFilter
    {
        // Size of the max prediction radius Can snap back to noisy data when too high
        private float MaxDeviationRadius = 0.04f;

        // How much smoothing will occur.  Will lag when too high
        private float Smoothing = 0.5f;

        // How much to correct back from prediction.  Can make things springy
        private float Correction = 0.5f;

        // Amount of prediction into the future to use. Can over shoot when too high
        private float Prediction = 0.5f;

        // Size of the radius where jitter is removed. Can do too much smoothing when too high
        private float JitterRadius = 0.05f;

        private FilterDoubleExponentialData[] history;

        private const float DEFAULT_MAX_DEV_RADIUS = 0.04f;
        private const float DEFAULT_SMOOTHING = 0.5f;
        private const float DEFAULT_CORRECTION = 0.5f;
        private const float DEFAULT_PREDICTION = 0.5f;
        private const float DEFAULT_JITTER_RADIUS = 0.05f;

        public SkeletonJointsPositionDoubleExponentialFilter()
        {
            MaxDeviationRadius = DEFAULT_MAX_DEV_RADIUS;
            Smoothing = DEFAULT_SMOOTHING;
            Correction = DEFAULT_CORRECTION;
            Prediction = DEFAULT_PREDICTION;
            JitterRadius = DEFAULT_JITTER_RADIUS;
            this.ResetHistory();
        }

        public SkeletonJointsPositionDoubleExponentialFilter(float smoothingValue, float correctionValue, 
            float predictionValue, float jitterRadiusValue, float maxDeviationRadiusValue)
        {
            MaxDeviationRadius = maxDeviationRadiusValue; 
            Smoothing = smoothingValue;                   
            Correction = correctionValue;                 
            Prediction = predictionValue;                 
            JitterRadius = jitterRadiusValue;             
            this.ResetHistory();
        }

        /// <summary>
        /// Reset the history data.
        /// </summary>
        public void ResetHistory()
        {
            Array jointTypeValues = Enum.GetValues(typeof(NUIJointType));
            this.history = new FilterDoubleExponentialData[jointTypeValues.Length];
        }

        public override bool UpdateParameters()
        {
            var result = false;

            EditorGUI.indentLevel++;
            var tempSmoothing = EditorGUILayout.Slider(new GUIContent("Smoothing"), Smoothing, 0f, 1f);
            if (tempSmoothing != Smoothing)
            {
                Smoothing = tempSmoothing;
                result = true;
            }

            var tempCorrection = EditorGUILayout.Slider(new GUIContent("Correction"), Correction, 0f, 1f);
            if (tempCorrection != Correction)
            {
                Correction = tempCorrection;
                result = true;
            }

            var tempPrediction = EditorGUILayout.Slider(new GUIContent("Prediction"), Prediction, 0f, 1f);
            if (tempPrediction != Prediction)
            {
                Prediction = tempPrediction;
                result = true;
            }

            var tempJitterRadius = EditorGUILayout.Slider(new GUIContent("JitterRadius"), JitterRadius, 0f, 1f);
            if (tempJitterRadius != JitterRadius)
            {
                JitterRadius = tempJitterRadius;
                result = true;
            }

            var tempMaxDeviationRadius = EditorGUILayout.Slider(new GUIContent("MaxDeviationRadius"), MaxDeviationRadius, 0f, 1f);
            if(tempMaxDeviationRadius != MaxDeviationRadius)
            {
                MaxDeviationRadius = tempMaxDeviationRadius;
                result = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button(new GUIContent("Reset to Default"), EditorStyles.miniButton))
            {
                Smoothing = DEFAULT_SMOOTHING;
                Correction = DEFAULT_CORRECTION;
                Prediction = DEFAULT_PREDICTION;
                JitterRadius = DEFAULT_JITTER_RADIUS;
                MaxDeviationRadius = DEFAULT_MAX_DEV_RADIUS;
                result = true;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            return result;
        }

        public override NUISkeleton Filter(CaptureCache cache)
        {
            List<NUISkeleton> skeletonCache = cache.GetCacheForFilter(this.Name);
            ResetHistory();
            NUISkeleton output = null;

            foreach (NUISkeleton skeleton in skeletonCache)
            {
                output = skeleton.Clone();

                Array jointTypeValues = Enum.GetValues(typeof(NUIJointType));
                JitterRadius = Math.Max(0.0001f, JitterRadius);

                float tempSmoothing = Smoothing;
                float tempCorrection = Correction;
                float tempPrediction = Prediction;
                float tempJitterRadius = JitterRadius;
                float tempMaxDeviationRadius = MaxDeviationRadius;

                foreach (NUIJointType jt in jointTypeValues)
                {
                    if (skeleton.Joints.ContainsKey(jt) && jt != NUIJointType.Unspecified)
                    {
                        if (skeleton.Joints[jt].Inferred)
                        {
                            tempJitterRadius *= 2.0f;
                            tempMaxDeviationRadius *= 2.0f;
                        }

                        output.Joints[jt].Position = this.FilterJoint(skeleton, jt, tempSmoothing, tempCorrection,
                            tempPrediction, tempJitterRadius, tempMaxDeviationRadius);
                    }
                }
            }

            return output;
        }

        protected Vector3 FilterJoint(NUISkeleton skeleton, NUIJointType jt, float smoothingValue, float correctionValue,
            float predictionValue, float jitterRadiusValue, float maxDeviationRadiusValue)
        {
            int jointIndex = (int)jt;

            Vector3 filteredPosition;
            Vector3 diffvec;
            Vector3 trend;
            float diffVal;

            Vector3 rawPosition = skeleton.Joints[jt].Position;
            Vector3 prevFilteredPosition = this.history[jointIndex].FilteredPosition;
            Vector3 prevTrend = this.history[jointIndex].Trend;
            Vector3 prevRawPosition = this.history[jointIndex].RawPosition;

            bool jointIsValid = rawPosition.x != 0.0f || rawPosition.y != 0.0f || rawPosition.z != 0.0f;
            if (!jointIsValid)
            {
                this.history[jointIndex].FrameCount = 0;
            }

            // Initial start values
            if (this.history[jointIndex].FrameCount == 0)
            {
                filteredPosition = rawPosition;
                trend = Vector3.zero;
            }
            else if (this.history[jointIndex].FrameCount == 1)
            {
                filteredPosition = (rawPosition + prevRawPosition) * 0.5f;
                diffvec = (filteredPosition - prevFilteredPosition);
                trend = (diffvec * correctionValue) + (prevTrend * (1.0f - correctionValue));
            }
            else
            {              
                // First apply jitter filter
                diffvec = (rawPosition - prevFilteredPosition);
                diffVal = Math.Abs(diffvec.magnitude);

                if (diffVal <= jitterRadiusValue)
                {
                    filteredPosition = rawPosition * (diffVal / jitterRadiusValue) + (prevFilteredPosition * (1.0f - (diffVal / jitterRadiusValue)));
                }
                else
                {
                    filteredPosition = rawPosition;
                }

                filteredPosition = (filteredPosition * (1.0f - smoothingValue)) + ((prevFilteredPosition + prevTrend) * smoothingValue);

                diffvec = (filteredPosition - prevFilteredPosition);
                trend = ((diffvec * correctionValue) + (prevTrend * (1.0f - correctionValue)));
            }

            Vector3 predictedPosition = (filteredPosition + (trend * predictionValue));

            diffvec = (predictedPosition - rawPosition);
            diffVal = Math.Abs(diffvec.magnitude);

            if (diffVal > maxDeviationRadiusValue)
            {
                predictedPosition = ((predictedPosition * (maxDeviationRadiusValue / diffVal)) + (rawPosition * (1.0f - (maxDeviationRadiusValue / diffVal))));
            }

            this.history[jointIndex].RawPosition = rawPosition;
            this.history[jointIndex].FilteredPosition = filteredPosition;
            this.history[jointIndex].Trend = trend;
            this.history[jointIndex].FrameCount++;
            
            return predictedPosition;
        }

        private struct FilterDoubleExponentialData
        {
            public Vector3 RawPosition { get; set; }

            public Vector3 FilteredPosition { get; set; }

            public Vector3 Trend { get; set; }

            public uint FrameCount { get; set; }
        }
    }
}