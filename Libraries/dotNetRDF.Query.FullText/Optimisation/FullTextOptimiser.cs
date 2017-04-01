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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An Algebra Optimiser that ensures that Full Text Query support is available to query evaluation
    /// </summary>
    public class FullTextOptimiser
        : IAlgebraOptimiser, IConfigurationSerializable
    {
        private IFullTextSearchProvider _provider;
        private IEnumerable<IPropertyFunctionFactory> _factories = new IPropertyFunctionFactory[] { new FullTextPropertyFunctionFactory() };

        /// <summary>
        /// Creates a Full Text Optimiser
        /// </summary>
        /// <param name="provider">Full Text Search Provider</param>
        public FullTextOptimiser(IFullTextSearchProvider provider)
        {
            if (provider == null) throw new ArgumentNullException("Full Text Search Provider cannot be null");
            this._provider = provider;
        }

        /// <summary>
        /// Optimises the Algebra to apply the <see cref="FullTextQuery"/> operator which ensures Full Text Query support is available to the query evaluation
        /// </summary>
        /// <param name="algebra">Algebra to optimise</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            return new FullTextQuery(this._provider, algebra);
        }

        /// <summary>
        /// Returns that the optimiser is applicable to all queries
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            q.PropertyFunctionFactories = q.PropertyFunctionFactories.Concat(this._factories);
            return true;
        }

        /// <summary>
        /// Returns that the optimiser is applicable to all updates
        /// </summary>
        /// <param name="cmds">Updates</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return true;
        }

        /// <summary>
        /// Serializes the Optimisers Configuration
        /// </summary>
        /// <param name="context">Serialization Context</param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            context.EnsureObjectFactory(typeof(FullTextObjectFactory));

            INode optObj = context.NextSubject;

            context.Graph.Assert(optObj, context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassAlgebraOptimiser)));
            context.Graph.Assert(optObj, context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)), context.Graph.CreateLiteralNode(this.GetType().FullName + ", dotNetRDF.Query.FullText"));

            if (this._provider is IConfigurationSerializable)
            {
                INode searcherObj = context.Graph.CreateBlankNode();
                context.NextSubject = searcherObj;
                ((IConfigurationSerializable)this._provider).SerializeConfiguration(context);
                context.Graph.Assert(optObj, context.Graph.CreateUriNode(UriFactory.Create(FullTextHelper.PropertySearcher)), searcherObj);
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to serialize configuration for this Full Text Optimiser as the Search Provider used does not implement the required IConfigurationSerializable interface");
            }
        }
    }
}
