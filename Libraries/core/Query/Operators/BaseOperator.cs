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

        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode op = context.NextSubject;
            INode opClass = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassSparqlOperator));

            context.Graph.Assert(op, context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), opClass);
            context.Graph.Assert(op, context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)), context.Graph.CreateLiteralNode(this.GetType().AssemblyQualifiedName));
        }
    }
}
