using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Operators;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// SPARQL Operator factory which is capable of loading any implementation of ISparqlOperator which has a public unparameterized constructor
    /// </summary>
    public class OperatorFactory
        : IObjectFactory
    {
        private Type _opType = typeof(ISparqlOperator);

        /// <summary>
        /// Tries to load an object of the given type
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Returned Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            Object temp = Activator.CreateInstance(targetType);
            if (temp is ISparqlOperator)
            {
                obj = (ISparqlOperator)temp;
                return true;
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to load the SPARQL Operator identified by the Node '" + objNode.ToString() + "' as the object could not be loaded as an object which implements the required ISparqlOperator interface");
            }
        }

        /// <summary>
        /// Gets whether this factory can load objects of the given type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            return t.GetInterfaces().Contains(this._opType) && t.GetConstructors().Any(c => c.GetParameters().Length == 0 && c.IsPublic);
        }
    }
}
