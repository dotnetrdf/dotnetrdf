using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.PropertyFunctions
{
    /// <summary>
    /// Factory Class 
    /// </summary>
    public static class PropertyFunctionFactory
    {
        private static List<IPropertyFunctionFactory> _factories;

        public static void AddFactory(IPropertyFunctionFactory factory)
        {
            lock (_factories)
            {
                _factories.Add(factory);
            }
        }

        public static void RemoveFactory(IPropertyFunctionFactory factory)
        {
            lock (_factories)
            {
                _factories.Remove(factory);
            }
        }

        public static bool IsPropertyFunction(Uri u)
        {
            return _factories.Any(f => f.IsPropertyFunction(u));
        }

        public static bool TryCreatePropertyFunction(PropertyFunctionInfo info, out IPropertyFunctionPattern function)
        {
            function = null;
            foreach (IPropertyFunctionFactory factory in _factories)
            {
                if (factory.TryCreatePropertyFunction(info, out function)) return true;
            }
            return false;
        }
    }
}
