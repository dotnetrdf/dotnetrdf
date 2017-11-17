/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Linq;
using VDS.RDF.Query.Operators;
#if NETCORE
using System.Reflection;
#endif

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// SPARQL Operator factory which is capable of loading any implementation of ISparqlOperator which has a public unparameterized constructor
    /// </summary>
    public class OperatorFactory
        : IObjectFactory
    {
        private Type _opType = typeof(ISparqlOperator);

        /// <summary>
        /// Tries to load an object of the given type
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Returned Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            Object temp = Activator.CreateInstance(targetType);
            if (temp is ISparqlOperator)
            {
                obj = (ISparqlOperator)temp;
                return true;
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to load the SPARQL Operator identified by the Node '" + objNode.ToString() + "' as the object could not be loaded as an object which implements the required ISparqlOperator interface");
            }
        }

        /// <summary>
        /// Gets whether this factory can load objects of the given type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            return t.GetInterfaces().Contains(_opType) && t.GetConstructors().Any(c => c.GetParameters().Length == 0 && c.IsPublic);
        }
    }
}
