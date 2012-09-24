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
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Operators
{
    /// <summary>
    /// Abstract base class for SPARQL Operators which also makes their configuration serializable
    /// </summary>
    public abstract class BaseOperator
        : ISparqlOperator, IConfigurationSerializable
    {
        /// <summary>
        /// Gets the operator this implementation represents
        /// </summary>
        public abstract SparqlOperatorType Operator
        {
            get;
        }

        /// <summary>
        /// Gets whether the operator can be applied to the given inputs
        /// </summary>
        /// <param name="ns">Inputs</param>
        /// <returns>True if applicable to the given inputs</returns>
        public abstract bool IsApplicable(params IValuedNode[] ns);

        /// <summary>
        /// Applies the operator
        /// </summary>
        /// <param name="ns">Inputs</param>
        /// <returns></returns>
        public abstract IValuedNode Apply(params IValuedNode[] ns);

        /// <summary>
        /// Serializes the configuration of the operator
        /// </summary>
        /// <param name="context">Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode op = context.NextSubject;
            INode opClass = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassSparqlOperator));

            context.Graph.Assert(op, context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), opClass);
            context.Graph.Assert(op, context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)), context.Graph.CreateLiteralNode(this.GetType().AssemblyQualifiedName));
        }
    }
}
