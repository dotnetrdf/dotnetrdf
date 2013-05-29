using System;

namespace VDS.RDF.Configuration
{
    public interface IConfigurationLoader
    {
        /// <summary>
        /// Loads the Object identified by the given URI as an object of the given type based on information from the Configuration Graph
        /// </summary>
        /// <remarks>
        /// See remarks under <see cref="ConfigurationLoader.LoadObject(VDS.RDF.IGraph,VDS.RDF.INode)"/> 
        /// </remarks>
        T LoadObject<T>(Uri objectIdentifier);
    }
}