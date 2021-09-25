/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Parsing;
using VDS.RDF.Query.Operators;

namespace VDS.RDF.Configuration
{
    public class SparqlConfigurationLoaderExtension : IConfigurationExtension
    {
        public const string PropertyEnabled = ConfigurationLoader.ConfigurationNamespace + "enabled";
        public const string ClassSparqlOperator = ConfigurationLoader.ConfigurationNamespace + "SparqlOperator";

        
        /// <summary>
        /// Given a Configuration Graph will detect and configure SPARQL Operators.
        /// </summary>
        /// <param name="g">Configuration Graph.</param>
        public static void AutoConfigureSparqlOperators(IGraph g)
        {
            INode rdfType = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfType)),
                operatorClass = g.CreateUriNode(g.UriFactory.Create(ClassSparqlOperator)),
                enabled = g.CreateUriNode(g.UriFactory.Create(PropertyEnabled));

            foreach (Triple t in g.GetTriplesWithPredicateObject(rdfType, operatorClass))
            {
                var temp = ConfigurationLoader.LoadObject(g, t.Subject);
                if (temp is ISparqlOperator)
                {
                    var enable = ConfigurationLoader.GetConfigurationBoolean(g, t.Subject, enabled, true);
                    if (enable)
                    {
                        SparqlOperators.AddOperator((ISparqlOperator)temp);
                    }
                    else
                    {
                        SparqlOperators.RemoveOperatorByType((ISparqlOperator)temp);
                    }
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-configuration of SPARQL Operators failed as the Operator specified by the Node '" + t.Subject.ToString() + "' does not implement the required ISparqlOperator interface");
                }
            }
        }

        public IEnumerable<Action<IGraph>> GetAutoConfigureActions() { yield return AutoConfigureSparqlOperators; }

        public IEnumerable<IObjectFactory> GetObjectFactories() { return ObjectFactories; }

        private static readonly List<IObjectFactory> ObjectFactories = new()
        {
            new ExpressionFactoryFactory(),
            new OperatorFactory(),
            new OptimiserFactory(),
            new PropertyFunctionFactoryFactory(),
            new QueryProcessorFactory(),
            new UpdateProcessorFactory(),
        };
    }
}
