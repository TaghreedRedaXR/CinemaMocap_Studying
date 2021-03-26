using CinemaSuite.CinemaMocap.System.Core.Mapping;
using System.Collections.Generic;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline
{
    public class CacheFrameData
    {
        public double elapsedTime;

        public NUISkeleton rawSkeleton;

        public List<FilterSkeleton> filterData = new List<FilterSkeleton>();

        public NUISkeleton result;

        public void Add(string filterName, NUISkeleton skeleton)
        {
            filterData.Add(new FilterSkeleton() { FilterName = filterName, Skeleton = skeleton });
        }

        public NUISkeleton CurrentSkeleton
        {
            get
            {
                if (filterData.Count == 0)
                {
                    return rawSkeleton;
                }
                return filterData[filterData.Count - 1].Skeleton;
            }
        }

        public NUISkeleton GetSkeletonBeforeFilter(string filterName)
        {
            NUISkeleton skeleton = CurrentSkeleton;

            for (int i = 0; i < filterData.Count; i++)
            {
                if (filterData[i].FilterName == filterName)
                {
                    if (i == 0)
                    {
                        skeleton = rawSkeleton;
                    }
                    else
                    {
                        skeleton = filterData[i - 1].Skeleton;
                    }
                }
            }

            return skeleton;
        }

        public class FilterSkeleton
        {
            public string FilterName;
            public NUISkeleton Skeleton;
        }

    }
}