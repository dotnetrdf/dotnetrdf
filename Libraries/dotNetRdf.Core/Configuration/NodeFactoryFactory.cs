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

using System;
using System.Linq;
using System.Reflection;

namespace VDS.RDF.Configuration;

/// <summary>
/// Configuration factory for creating <see cref="NodeFactory"/> instances.
/// </summary>
public class NodeFactoryFactory : IObjectFactory
{
    /// <inheritdoc />
    public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
    {
        obj = null;
        try
        {
            var nodeFactory = (INodeFactory)Activator.CreateInstance(targetType);
            if (nodeFactory != null)
            {
                ConfigureBaseUri(g, objNode, nodeFactory);
                ConfigureNormalizeLiteralNodes(g, objNode, nodeFactory);
                ConfigureLanguageTagValidation(g, objNode, nodeFactory);
                ConfigureUriFactory(g, objNode, nodeFactory);
            }
            obj = nodeFactory;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public bool CanLoadObject(Type t)
    {
        Type nodeFactoryType = typeof(INodeFactory);
        if (t.GetInterfaces().Any(i => i == nodeFactoryType))
        {
            ConstructorInfo c = t.GetConstructor([]);
            return c != null && c.IsPublic;
        }
        return false;
    }

    private static void ConfigureBaseUri(IGraph g, INode objNode, INodeFactory nodeFactory)
    {
        INode baseUriNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyAssignUri)));
        switch (baseUriNode)
        {
            case null:
                return;
            case IUriNode uriNode:
                nodeFactory.BaseUri = uriNode.Uri;
                break;
            case ILiteralNode literalNode:
                nodeFactory.BaseUri = g.UriFactory.Create(literalNode.Value);
                break;
            default:
                return;
        }
    }

    private static void ConfigureNormalizeLiteralNodes(IGraph g, INode objNode, INodeFactory nodeFactory)
    {
        nodeFactory.NormalizeLiteralValues = ConfigurationLoader.GetConfigurationBoolean(g, objNode,
            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyNormalizeLiterals)), false);
    }

    private static void ConfigureLanguageTagValidation(IGraph g, INode objNode, INodeFactory nodeFactory)
    {
        var validationModeName =
            ConfigurationLoader.GetConfigurationString(g, objNode, 
                g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyWithLanguageTagValidation)));
        if (validationModeName == null) { return; }
        switch (validationModeName.ToLowerInvariant())
        {
            case "true":
            case "turtle":
                nodeFactory.LanguageTagValidation = LanguageTagValidationMode.Turtle;
                break;
            case "false":
            case "none":
                nodeFactory.LanguageTagValidation = LanguageTagValidationMode.None;
                break;
            case "wellformed":
            case "bcp47":
                nodeFactory.LanguageTagValidation = LanguageTagValidationMode.WellFormed;
                break;
            default:
                throw new DotNetRdfConfigurationException(
                    $"Unable to configure INodeFactory instance for node {objNode.ToString()}. The value of the property dnr:withLanguageTagValidation is not recognized. Expected turtle, true, wellformed, bcp47, none or false. Found {validationModeName}.");
        }
    }

    private static void ConfigureUriFactory(IGraph g, INode objNode, INodeFactory nodeFactory)
    {
        INode uriFactoryNode = ConfigurationLoader.GetConfigurationNode(g, objNode,
            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingUriFactory)));
        if (uriFactoryNode != null)
        {
            var uriFactory = ConfigurationLoader.LoadObject(g, uriFactoryNode) as IUriFactory;
            if (uriFactory != null)
            {
                nodeFactory.UriFactory = uriFactory;
            }
        }
    }

}