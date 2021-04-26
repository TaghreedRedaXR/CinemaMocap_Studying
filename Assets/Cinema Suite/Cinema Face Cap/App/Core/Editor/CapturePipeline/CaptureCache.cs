
using System;
using System.Collections.Generic;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline
{
    public class CaptureCache
    {
        private List<CacheFrameData> frameData;
        private int cacheSize = 10;

        public CaptureCache()
        {
            frameData = new List<CacheFrameData>();
        }

        internal void AddNewFrame(MappedFaceCapFrame face, double totalMilliseconds)
        {
            var data = new CacheFrameData();
            data.rawFace = new MappedFaceCapFrame(face);
            data.elapsedTime = totalMilliseconds;

            frameData.Add(data);

            if (frameData.Count > cacheSize)
            {
                frameData.RemoveAt(0);
            }
        }

        internal void AddFiltered(string name, MappedFaceCapFrame filtered)
        {
            CurrentFrameData.Add(name, filtered);
        }

        internal void AddMapped(MappedFaceCapFrame mapped)
        {
            CurrentFrameData.Add("Mapped", mapped);
        }

        internal void AddResult(MappedFaceCapFrame filtered)
        {
            CurrentFrameData.Add("Result", filtered);
        }

        internal MappedFaceCapFrame GetResult(int index)
        {
            return frameData[index].CurrentFace;
        }

        public CacheFrameData CurrentFrameData
        {
            get
            {
                return frameData[frameData.Count - 1];
            }
        }

        public MappedFaceCapFrame CurrentFace
        {
            get
            {
                return frameData[frameData.Count - 1].CurrentFace;
            }
        }

        public int CacheSize
        {
            get { return cacheSize; }
            set { cacheSize = value; }
        }

        internal List<MappedFaceCapFrame> GetCacheAfterFilter(string name)
        {
            var faces = new List<MappedFaceCapFrame>();

            foreach (CacheFrameData data in frameData)
            {
                var face = data.GetFaceAfterFilter(name);
                if (face != null)
                {
                    faces.Add(face);
                }
            }

            return faces;
        }

        internal List<MappedFaceCapFrame> GetCacheForFilter(string name)
        {
            var faces = new List<MappedFaceCapFrame>();

            foreach (CacheFrameData data in frameData)
            {
                var face = data.GetFaceBeforeFilter(name);
                if (face != null)
                {
                    faces.Add(face);
                }
            }

            return faces;
        }
    }
}