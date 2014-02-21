using System;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Interface for Object Factories which are factory classes that can create Objects based on configuration information in a Graph
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Attempts to load an Object of the given type identified by the given Node and returned as the Type that this loader generates
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Created Object</param>
        /// <returns>True if the loader succeeded in creating an Object</returns>
        /// <remarks>
        /// <para>
        /// The Factory should not throw an error if some required configuration is missing as another factory further down the processing chain may still be able to create the object.  If the factory encounters errors and all the required configuration information is present then that error should be thrown i.e. class instantiation throws an error or a call to load an object that this object requires fails.
        /// </para>
        /// </remarks>
        bool TryLoadObject(IGraph g, INode objNode, Type targetType, out Object obj);

        /// <summary>
        /// Returns whether this Factory is capable of creating objects of the given type
        /// </summary>
        /// <param name="t">Target Type</param>
        /// <returns></returns>
        bool CanLoadObject(Type t);
    }
}