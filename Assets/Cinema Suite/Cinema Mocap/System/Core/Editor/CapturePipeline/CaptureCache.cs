using CinemaSuite.CinemaMocap.System.Core.Mapping;
using System.Collections.Generic;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline
{
    public class CaptureCache
    {
        private List<CacheFrameData> frameData;
        private int cacheSize = 5;

        public CaptureCache()
        {
            frameData = new List<CacheFrameData>();
        }

        internal void AddNewFrame(NUISkeleton skeleton, double totalMilliseconds)
        {
            var data = new CacheFrameData();
            data.rawSkeleton = skeleton;
            data.elapsedTime = totalMilliseconds;

            frameData.Add(data);

            if (frameData.Count > cacheSize)
            {
                frameData.RemoveAt(0);
            }
        }

        internal void AddFiltered(string name, NUISkeleton filtered)
        {
            CurrentFrameData.Add(name, filtered);
        }

        internal void AddMapped(NUISkeleton mapped)
        {
            CurrentFrameData.Add("Mapped", mapped);
        }

        internal void AddResult(NUISkeleton filtered)
        {
            CurrentFrameData.Add("Result", filtered);
        }

        public CacheFrameData CurrentFrameData
        {
            get
            {
                return frameData[frameData.Count - 1];
            }
        }

        public NUISkeleton CurrentSkeleton
        {
            get
            {
                return frameData[frameData.Count - 1].CurrentSkeleton;
            }
        }

        public int CacheSize
        {
            get
            {
                return cacheSize;
            }

            set
            {
                cacheSize = value;
            }
        }

        internal List<NUISkeleton> GetCacheForFilter(string name)
        {
            var skeletons = new List<NUISkeleton>();

            foreach (CacheFrameData data in frameData)
            {
                var skeleton = data.GetSkeletonBeforeFilter(name);
                if (skeleton != null)
                {
                    skeletons.Add(skeleton);
                }
            }

            return skeletons;
        }
    }
}