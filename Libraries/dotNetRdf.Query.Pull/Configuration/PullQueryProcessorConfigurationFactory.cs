/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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