using CinemaSuite.CinemaFaceCap.App.Core.Editor.Utility;
using CinemaSuite.CinemaFaceCap.App.Core.Mapping;
using System;
using System.Collections.Generic;

namespace CinemaSuite.CinemaFaceCap.App.Core.Editor.CapturePipeline
{

    public class CaptureFilterAttribute : Attribute
    {
        private bool preMapping = false;

        public CaptureFilterAttribute(bool preMapping)
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

    public abstract class CaptureFilter
    {
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public int Ordinal { get; set; }

        public bool PreMapping { get; set; }

        public abstract string ENABLED_KEY { get; }

        public abstract string ORDINAL_KEY { get; }

        public virtual bool UpdateParameters() { return false; }

        public virtual MappedFaceCapFrame Filter(CaptureCache cache)
        {
            return Filter(cache.CurrentFace);
        }

        public virtual MappedFaceCapFrame Filter(MappedFaceCapFrame input)
        {
            return input;
        }

        /// <summary>
        /// Create instances of all Mocap Filters in the assembly and return the collection.
        /// </summary>
        /// <returns>The collection of filters</returns>
        public static List<CaptureFilter> loadAvailableFilters()
        {
            List<CaptureFilter> filters = new List<CaptureFilter>();

            List<Type> types = Utility.Helper.GetFilters();
            foreach (Type t in types)
            {
                CaptureFilter filter = Activator.CreateInstance(t) as CaptureFilter;
                if(filter != null)
                {
                    filters.Add(filter);

                    foreach (NameAttribute attribute in t.GetCustomAttributes(typeof(NameAttribute), true))
                    {
                        filter.Name = attribute.Name;
                    }

                    foreach (CaptureFilterAttribute attribute in t.GetCustomAttributes(typeof(CaptureFilterAttribute), true))
                    {
                        filter.PreMapping = attribute.PreMapping;
                    }
                    if (filter.Ordinal == default(int)) // If the ordinal has not already been set from EditorPrefs
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
