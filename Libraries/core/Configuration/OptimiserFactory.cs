/*

Copyright Robert Vesse 2009-11
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
using System.Linq;
using VDS.RDF.Query.Optimisation;

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
                    INode statsObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingGraph));
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
                    //Try and create an Algebra Optimiser
                    try
                    {
                        obj = (IAlgebraOptimiser)Activator.CreateInstance(targetType);
                    }
                    catch
                    {
                        //Any error means this loader can't load this type
                        return false;
                    }
                    break;
            }

            //Return true only if we've loaded something into the output object
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
