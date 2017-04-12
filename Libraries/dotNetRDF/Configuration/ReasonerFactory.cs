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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDS.RDF.Query.Inference;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing Reasoners from Configuration Graphs
    /// </summary>
    public class ReasonerFactory : IObjectFactory
    {
        private const String OwlReasonerWrapperType = "VDS.RDF.Query.Inference.OwlReasonerWrapper";
        private const String PelletReasonerType = "VDS.RDF.Query.Inference.PelletReasoner";

        /// <summary>
        /// Tries to load a Reasoner based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            Object output;

            switch (targetType.FullName)
            {
                case PelletReasonerType:
                    String server = ConfigurationLoader.GetConfigurationValue(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer)));
                    if (server == null) return false;
                    String kb = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStore)));
                    if (kb == null) return false;

                    output = new PelletReasoner(UriFactory.Create(server), kb);
                    break;

                case OwlReasonerWrapperType:
                    INode reasonerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyOwlReasoner)));
                    if (reasonerNode == null) return false;
                    Object reasoner = ConfigurationLoader.LoadObject(g, reasonerNode);
                    if (reasoner is IOwlReasoner)
                    {
                        output = new OwlReasonerWrapper((IOwlReasoner)reasoner);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load configuration for the OWL Reasoner Wrapper identified by the Node '" + objNode.ToString() + "' as the value for the dnr:owlReasoner property points to an Object which cannot be loaded as an object which implements the IOwlReasoner interface");
                    }
                    break;

                default:
                    // Otherwise we'll just attempt to load this as an instance of an IInferenceEngine
                    try
                    {
                        output = (IInferenceEngine)Activator.CreateInstance(targetType);
                    }
                    catch
                    {
                        // Any error means this loader can't load this type
                        return false;
                    }
                    break;
            }

            if (output != null)
            {
                if (output is IInferenceEngine)
                {
                    // Now initialise with any specified Graphs
                    IEnumerable<INode> rulesGraphs = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingGraph)));
                    foreach (INode rulesGraph in rulesGraphs)
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, rulesGraph);
                        if (temp is IGraph)
                        {
                            ((IInferenceEngine)output).Initialise((IGraph)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load Configuration for the Forward Chaining Reasoner identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:usingGraph property points to an Object which cannot be loaded as an object which implements the IGraph interface");
                        }
                    }
                }
            }

            obj = output;
            return true;
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            Type ireasoner = typeof(IInferenceEngine);

            // We can load any object which implements IInferenceEngine and has a public unparameterized constructor
            if (t.GetInterfaces().Any(i => i.Equals(ireasoner)))
            {
                ConstructorInfo c = t.GetConstructor(new Type[0]);
                if (c != null)
                {
                    if (c.IsPublic) return true;
                }
            }

            // We can also load some other types of reasoner
            switch (t.FullName)
            {
                case OwlReasonerWrapperType:
                case PelletReasonerType:
                    return true;
                default:
                    return false;
            }
        }
    }
}
