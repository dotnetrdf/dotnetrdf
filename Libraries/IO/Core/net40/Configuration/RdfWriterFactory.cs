using System;
using System.Linq;
using System.Reflection;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Object Factory used by the Configuration API to load writers from configuration graphs
    /// </summary>
    public class RdfWriterFactory : IObjectFactory
    {
        private readonly Type _rdfWriterType = typeof (IRdfWriter);

        /// <summary>
        /// Tries to load a Writer based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            try
            {
                obj = Activator.CreateInstance(targetType);
                return true;
            }
            catch
            {
                //Any error means this loader can't load this type
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
            //We can load any object which implements any writer interface and has a public unparameterized constructor
            if (t.GetInterfaces().Any(i => this._rdfWriterType.Equals(i)))
            {
                ConstructorInfo c = t.GetConstructor(new Type[0]);
                if (c != null)
                {
                    return c.IsPublic;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}