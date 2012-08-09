/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

            context.Graph.Assert(optObj, context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassAlgebraOptimiser));
            context.Graph.Assert(optObj, ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType), context.Graph.CreateLiteralNode(this.GetType().FullName + ", dotNetRDF.Query.FullText"));

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
