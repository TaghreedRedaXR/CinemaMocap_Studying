
using CinemaSuite;
using CinemaSuite.CinemaMocap.System.Core.Editor.Utility;
using CinemaSuite.CinemaMocap.System.Core.Mapping;
using System;
using System.Collections.Generic;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.CapturePipeline
{
    public class MocapFilterAttribute : Attribute
    {
        private bool preMapping = false;

        public MocapFilterAttribute(bool preMapping)
        {
            this.preMapping = preMapping;
        }

        public bool PreMapping
        {
            get
            {
                return preMapping;
            }
        }
    }

    public abstract class MocapFilter
    {
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public int Ordinal { get; set; }

        public bool PreMapping { get; set; }

        public virtual NUISkeleton Filter(CaptureCache cache)
        {
            NUISkeleton skeleton = cache.CurrentSkeleton;
            return Filter(skeleton);
        }

        public virtual NUISkeleton Filter(NUISkeleton input) { return input; }

        public virtual bool UpdateParameters()
        { return false; }

        /// <summary>
        /// Create instances of all Mocap Filters in the assembly and return the collection.
        /// </summary>
        /// <returns>The collection of filters</returns>
        public static List<MocapFilter> loadAvailableFilters()
        {
            List<MocapFilter> filters = new List<MocapFilter>();

            List<Type> types = CinemaMocapHelper.GetFilters();
            foreach (Type t in types)
            {
                MocapFilter filter = Activator.CreateInstance(t) as MocapFilter;
                if(filter != null)
                {
                    filters.Add(filter);

                    foreach (NameAttribute attribute in t.GetCustomAttributes(typeof(NameAttribute), true))
                    {
                        filter.Name = attribute.Name;
                    }

                    foreach (MocapFilterAttribute attribute in t.GetCustomAttributes(typeof(MocapFilterAttribute), true))
                    {
                        filter.PreMapping = attribute.PreMapping;
                    }
                    foreach (OrdinalAttribute attribute in t.GetCustomAttributes(typeof(OrdinalAttribute), true))
                    {
                        filter.Ordinal = attribute.Ordinal;
                    }
                }
            }

            return filters;
        }
    }
}
