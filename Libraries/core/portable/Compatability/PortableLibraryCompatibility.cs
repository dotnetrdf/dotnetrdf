using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    public class CategoryAttribute : Attribute
    {
        public CategoryAttribute(string name) { }
    }

    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string desc) { }
    }

    public class ReadOnlyAttribute : Attribute
    {
        public ReadOnlyAttribute(bool readOnly)
        {
        }
    }

    public class TypeConverterAttribute : Attribute
    {
        public TypeConverterAttribute(Type converterType){}
    }
}
