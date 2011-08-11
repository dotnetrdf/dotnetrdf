/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
#if !SILVERLIGHT
        private const String PelletReasonerType = "VDS.RDF.Query.Inference.PelletReasoner";
#endif

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
#if !SILVERLIGHT
                case PelletReasonerType:
                    String server = ConfigurationLoader.GetConfigurationValue(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyServer));
                    if (server == null) return false;
                    String kb = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyStore));
                    if (kb == null) return false;

                    output = new PelletReasoner(new Uri(server), kb);
                    break;
#endif

                case OwlReasonerWrapperType:
                    INode reasonerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyOwlReasoner));
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
                    //Otherwise we'll just attempt to load this as an instance of an IInferenceEngine
                    try
                    {
                        output = (IInferenceEngine)Activator.CreateInstance(targetType);
                    }
                    catch
                    {
                        //Any error means this loader can't load this type
                        return false;
                    }
                    break;
            }

            if (output != null)
            {
                if (output is IInferenceEngine)
                {
                    //Now initialise with any specified Graphs
                    IEnumerable<INode> rulesGraphs = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingGraph));
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

            //We can load any object which implements IInferenceEngine and has a public unparameterized constructor
            if (t.GetInterfaces().Any(i => i.Equals(ireasoner)))
            {
                ConstructorInfo c = t.GetConstructor(System.Type.EmptyTypes);
                if (c != null)
                {
                    if (c.IsPublic) return true;
                }
            }

            //We can also load some other types of reasoner
            switch (t.FullName)
            {
                case OwlReasonerWrapperType:
#if !SILVERLIGHT
                case PelletReasonerType:
#endif
                    return true;
                default:
                    return false;
            }
        }
    }
}
