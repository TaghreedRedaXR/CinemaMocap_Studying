using System;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.Utility
{
    /// <summary>
    /// A helper class for storing custom data for drop downs.
    /// </summary>
    public class TypeLabelContextData
    {
        // The data type
        public Type Type; 

        // The user friendly name
        public string Label;

        // An Id for ordering (optional)
        public int Ordinal;

        public TypeLabelContextData(Type type, string label)
        {
            Type = type;
            Label = label;
        }

        public TypeLabelContextData(Type type, string label, int ordinal)
        {
            Type = type;
            Label = label;
            Ordinal = ordinal;
        }
    }
}
