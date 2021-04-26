using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using System.Collections.Generic;
using System;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline
{
    public class FilterFace
    {
        public string FilterName;
        public MappedFaceCapFrame Face;
    }

    public class CacheFrameData
    {
        public double elapsedTime;

        public MappedFaceCapFrame rawFace;
        public List<FilterFace> filterData = new List<FilterFace>();

        public void Add(string filterName, MappedFaceCapFrame face)
        {
            filterData.Add(new FilterFace() { FilterName = filterName, Face = face });
        }

        public MappedFaceCapFrame CurrentFace
        {
            get
            {
                if (filterData.Count == 0)
                {
                    return rawFace;
                }
                return filterData[filterData.Count - 1].Face;
            }
        }

        public MappedFaceCapFrame GetFaceBeforeFilter(string filterName)
        {
            var face = CurrentFace;

            for (int i = 0; i < filterData.Count; i++)
            {
                if (filterData[i].FilterName == filterName)
                {
                    if (i == 0)
                    {
                        face = rawFace;
                    }
                    else
                    {
                        face = filterData[i - 1].Face;
                    }
                }
            }

            return face;
        }

        internal MappedFaceCapFrame GetFaceAfterFilter(string name)
        {
            MappedFaceCapFrame face = null;

            for (int i = 0; i < filterData.Count; i++)
            {
                if (filterData[i].FilterName == name)
                {
                    face = filterData[i].Face;
                }
            }

            return face;
        }
    }
}