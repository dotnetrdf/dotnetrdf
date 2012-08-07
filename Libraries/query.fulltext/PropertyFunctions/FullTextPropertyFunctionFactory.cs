using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.PropertyFunctions
{
    /// <summary>
    /// Property Function factory for Full Text functions
    /// </summary>
    public class FullTextPropertyFunctionFactory
        : IPropertyFunctionFactory
    {
        /// <summary>
        /// Gets whether the given URI is a property function URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public bool IsPropertyFunction(Uri u)
        {
            switch (u.AbsoluteUri)
            {
                case FullTextHelper.FullTextMatchPredicateUri:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Tries to create property functions
        /// </summary>
        /// <param name="info">Function information</param>
        /// <param name="function">Property Function</param>
        /// <returns></returns>
        public bool TryCreatePropertyFunction(PropertyFunctionInfo info, out IPropertyFunctionPattern function)
        {
            function = null;
            switch (info.FunctionUri.AbsoluteUri)
            {
                case FullTextHelper.FullTextMatchPredicateUri:
                    function = new PropertyFunctionPattern(info, new FullTextMatchPropertyFunction(info));
                    return true;
                default:
                    return false;
            }
        }
    }
}
