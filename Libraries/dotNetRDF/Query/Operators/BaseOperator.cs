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
            context.Graph.Assert(op, context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)), context.Graph.CreateLiteralNode(GetType().AssemblyQualifiedName));
        }
    }
}
