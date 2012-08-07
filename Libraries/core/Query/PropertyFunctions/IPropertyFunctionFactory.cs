using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.PropertyFunctions
{
    public interface IPropertyFunctionFactory
    {
        bool IsPropertyFunction(Uri u);

        bool TryCreatePropertyFunction(PropertyFunctionInfo info, out IPropertyFunctionPattern function);
    }
}
