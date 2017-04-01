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
using VDS.RDF.Query.Optimisation;
#if NETCORE
using System.Reflection;
#endif

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// An Object Factory that can generate SPARQL Query and Algebra Optimisers
    /// </summary>
    public class OptimiserFactory
        : IObjectFactory
    {
        private const String QueryOptimiserDefault = "VDS.RDF.Query.Optimisation.DefaultOptimiser";
        private const String QueryOptimiserNoReorder = "VDS.RDF.Query.Optimisation.NoReorderOptimiser";
        private const String QueryOptimiserWeighted = "VDS.RDF.Query.Optimisation.WeightedOptimiser";

        /// <summary>
        /// Tries to load a SPARQL Query/Algebra Optimiser based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            Object temp;

            switch (targetType.FullName)
            {
                case QueryOptimiserDefault:
                    obj = new DefaultOptimiser();
                    break;

                case QueryOptimiserNoReorder:
                    obj = new NoReorderOptimiser();
                    break;

                case QueryOptimiserWeighted:
                    INode statsObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingGraph)));
                    if (statsObj != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, statsObj);
                        if (temp is IGraph)
                        {
                            obj = new WeightedOptimiser((IGraph)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to create the Weighted Query Optimiser identified by the Node '" + objNode.ToString() + "' since the dnr:usingGraph property points to an object that cannot be loaded as an Object that imlements the required IGraph interface");
                        }
                    }
                    else
                    {
                        obj = new WeightedOptimiser();
                    }
                    break;

                default:
                    // Try and create an Algebra Optimiser
                    try
                    {
                        obj = (IAlgebraOptimiser)Activator.CreateInstance(targetType);
                    }
                    catch
                    {
                        // Any error means this loader can't load this type
                        return false;
                    }
                    break;
            }

            // Return true only if we've loaded something into the output object
            if (obj != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case QueryOptimiserDefault:
                case QueryOptimiserNoReorder:
                case QueryOptimiserWeighted:
                    return true;
                default:
                    Type algOptType = typeof(IAlgebraOptimiser);
                    return t.GetInterfaces().Any(i => i.Equals(algOptType));
            }
        }
    }
}
