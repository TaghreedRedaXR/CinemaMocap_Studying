using System;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.Utility
{
    /// <summary>
    /// An attribute for defining a user friendly name for a class.
    /// </summary>
    public class NameAttribute : Attribute
    {
        private string name = "Name";

        public NameAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Get the user friendly name from this attribute.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }

    /// <summary>
    /// An attribute for ordering classes used in collections.
    /// </summary>
    public class OrdinalAttribute : Attribute
    {
        // For ordering
        private int ordinal = 0;

        public OrdinalAttribute(int ordinal)
        {
            this.ordinal = ordinal;
        }

        /// <summary>
        /// The ordinal value of this attribute.
        /// </summary>
        public int Ordinal
        {
            get
            {
                return ordinal;
            }
        }
    }
}
