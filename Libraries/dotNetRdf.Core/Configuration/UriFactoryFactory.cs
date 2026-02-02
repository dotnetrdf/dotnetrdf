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
/// An object factory for constructing <see cref="IUriFactory"/> instances.
/// </summary>
public class UriFactoryFactory : IObjectFactory
{
    /// <inheritdoc />
    public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
    {
        try
        {
            INode parentNode = ConfigurationLoader.GetConfigurationNode(g, objNode,
                g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyWithParent)));
            IUriFactory parentFactory = null;
            if (parentNode != null)
            {
                parentFactory = (IUriFactory)ConfigurationLoader.LoadObject(g, parentNode);
            }

            obj = Activator.CreateInstance(targetType, parentFactory ?? UriFactory.Root);
            ConfigureInternUris(g, objNode, obj as IUriFactory);
            return true;
        }
        catch
        {
            obj = null;
            return false;
        }

    }

    /// <inheritdoc />
    public bool CanLoadObject(Type t)
    {
        Type iUriFactory = typeof(IUriFactory);
        if (t.GetInterfaces().Any(i => i == iUriFactory))
        {
            ConstructorInfo ctor = t.GetConstructor([typeof(IUriFactory)]);
            return ctor != null && ctor.IsPublic;
        }

        return false;
    }

    private static void ConfigureInternUris(IGraph g, INode objNode, IUriFactory obj)
    {
        var internUris = ConfigurationLoader.GetConfigurationBoolean(g, objNode,
            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyInternUris)), true);
        obj.InternUris = internUris;
    }
}