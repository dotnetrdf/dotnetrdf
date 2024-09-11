using VDS.RDF.Configuration;

namespace VDS.RDF.Query.Pull.Configuration;

/// <summary>
/// Factory class for producing a <see cref="PullQueryProcessor"/> from a configuration graph.
/// </summary>
public class PullQueryProcessorConfigurationFactory : IObjectFactory
{
    private const string PullQueryProcessor = "VDS.RDF.Query.Pull.PullQueryProcessor";
    

    /// <inheritdoc />
    public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object? obj)
    {
        obj = null;
        ISparqlQueryProcessor? queryProcessor = null;

        // Get the property nodes we will use to load the configuration
        INode usingStore = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingStore));
        switch (targetType.FullName)
        {
            case PullQueryProcessor:
                INode storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, usingStore);
                if (storeObj == null) return false;
                var store = ConfigurationLoader.LoadObject(g, storeObj);
                if (store is ITripleStore tripletStoreObj)
                {
                    queryProcessor = new PullQueryProcessor(tripletStoreObj, options =>
                    {
                        ApplyConfigurationOptions(options, g, objNode);
                    });
                }
                break;
        }
        obj = queryProcessor;
        return queryProcessor != null;
    }

    private void ApplyConfigurationOptions(PullQueryOptions options, IGraph g, INode objNode)
    {
        INode? timeoutNode = g.GetUriNode(UriFactory.Create(ConfigurationLoader.PropertyTimeout));
        if (timeoutNode != null)
        {
            var timeout = ConfigurationLoader.GetConfigurationUInt64(g, objNode, timeoutNode, PullQueryOptions.DefaultQueryExecutionTimeout);
            options.QueryExecutionTimeout = timeout;
        }
        
        INode? unionDefaultGraphNode = g.GetUriNode(UriFactory.Create(ConfigurationLoader.PropertyUnionDefaultGraph));
        if (unionDefaultGraphNode != null)
        {
            var unionDefaultGraph = ConfigurationLoader.GetConfigurationBoolean(g, objNode, unionDefaultGraphNode, false);
            options.UnionDefaultGraph = unionDefaultGraph;
        }
    }
    
    /// <inheritdoc />
    public bool CanLoadObject(Type t)
    {
        switch (t.FullName)
        {
            case PullQueryProcessor:
                return true;
            default:
                return false;
        }
    }
}