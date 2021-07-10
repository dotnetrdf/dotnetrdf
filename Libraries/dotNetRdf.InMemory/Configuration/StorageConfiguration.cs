using System;
using System.Collections.Generic;

namespace VDS.RDF.Configuration
{
    public class InMemoryConfigurationLoaderExtension : IConfigurationExtension
    {
        private static readonly List<IObjectFactory> ObjectFactories = new()
        {
            new DatasetFactory(),
            new InMemoryUpdateProcessorFactory(),
            new InMemoryUpdateProcessorFactory(),
            new InMemoryStorageFactory(),
            new StoreFactory(),
        };


        public IEnumerable<IObjectFactory> GetObjectFactories()
        {
            return ObjectFactories;
        }

        public IEnumerable<Action<IGraph>> GetAutoConfigureActions()
        {
            yield break;
        }
    }
}
