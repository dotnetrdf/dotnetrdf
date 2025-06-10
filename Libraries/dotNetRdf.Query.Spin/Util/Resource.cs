/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin.Model;
using System.Reflection;

namespace VDS.RDF.Query.Spin.Util;

/// <summary>
/// A utility class that wraps dotNetRDF Nodes with the same interface as the Jena Resource classes
/// </summary>
internal class Resource : WrapperNode, IResource, IComparable<IResource>, IEquatable<IResource>
{

    #region Basic resource wrapper implementation

    private readonly SpinProcessor _model;

    internal static Resource Get(INode node, IGraph inferenceGraph, SpinProcessor spinModel)
    {
        if (node == null) return null;
        if (node is Resource resource && resource.getModel() == spinModel)
        {
            return resource;
        }
        return new Resource(node, inferenceGraph, spinModel);
    }

    protected Resource(INode node, IGraph inferenceGraph, SpinProcessor spinModel) : base(node is Resource resourceNode ? resourceNode.Node : node)
    {
        Graph = inferenceGraph;
        _model = spinModel;
    }

    public IGraph Graph { get; }

    public bool isBlank()
    {
        return Node is IBlankNode;
    }

    public bool isUri()
    {
        return Node is IUriNode;
    }

    public bool isLiteral()
    {
        return Node is ILiteralNode;
    }

    public Uri Uri
    {
        get
        {
            if (isUri())
            {
                return ((IUriNode)Node).Uri;
            }
            return null;
        }
    }

    public INode getSource()
    {
        return Node;
    }

    public SpinProcessor getModel()
    {
        return _model;
    }

    public bool canAs(INode cls)
    {
        return _model.ContainsTriple(Node, RDF.PropertyType, cls);
    }

    // A constructor cache to optimize half of the reflection work
    private static readonly Dictionary<Type, ConstructorInfo> constructors = new Dictionary<Type, ConstructorInfo>();
    public IResource As(Type cls)
    {
        ConstructorInfo constructor;
        if (constructors.ContainsKey(cls))
        {
            constructor = constructors[cls];
        }
        else
        {
            constructor = cls.GetConstructor(new[] { typeof(INode), typeof(IGraph), typeof(SpinProcessor) });
            constructors[cls] = constructor;
        }

        if (constructor == null)
        {
            throw new SpinException("Unable to locate a usable constructor for type " + cls.FullName);
        }
        return (IResource)constructor.Invoke(new object[] { Node, Graph, _model });
    }

    public IEnumerable<IResource> getObjects(INode property)
    {
        return _model.GetTriplesWithSubjectPredicate(this, property).Select(t => Resource.Get(t.Object, Graph, _model));
    }

    public List<IResource> AsList()
    {
        var result = new List<IResource>();
        INode listRoot = this;
        Triple step = _model.GetTriplesWithSubjectPredicate(listRoot, RDF.PropertyFirst).FirstOrDefault();
        if (step != null)
        {
            while (step != null)
            {
                if (!RDFUtil.sameTerm(RDF.Nil, step.Object))
                {
                    result.Add(Resource.Get(step.Object, Graph, _model));
                }
                step = _model.GetTriplesWithSubjectPredicate(listRoot, RDF.PropertyRest).FirstOrDefault();
                if (step != null)
                {
                    if (RDFUtil.sameTerm(RDF.Nil, step.Object))
                    {
                        break;
                    }
                    listRoot = step.Object;
                    step = _model.GetTriplesWithSubjectPredicate(listRoot, RDF.PropertyFirst).FirstOrDefault();
                }
            }
        }
        else
        {
            result.Add(this);
        }
        return result;
    }

    public void AddProperty(INode predicate, INode value)
    {
        if (predicate == null || value == null)
        {
            return;
        }
        if (predicate is IResource predicateResource)
        {
            predicate = predicateResource.getSource();
        }
        if (value is IResource valueResource)
        {
            value = valueResource.getSource();
        }
        Graph.Assert(Node, predicate, value);
    }

    // TODO check whether we need to reference Model or _model
    public IEnumerable<Triple> listProperties()
    {
        return _model.GetTriplesWithSubject(Node);
    }

    public IEnumerable<Triple> listProperties(INode property)
    {
        return _model.GetTriplesWithSubjectPredicate(Node, property);
    }

    public bool hasProperty(INode property)
    {
        return _model.GetTriplesWithSubjectPredicate(Node, property).Any();
    }

    public bool hasProperty(INode property, INode value)
    {
        return _model.ContainsTriple(Node, property, value);
    }

    /* To simplify subsequent code and calls, we consider chaining cases where property is null */
    public Triple getProperty(INode property)
    {
        if (property != null)
        {
            return _model.GetTriplesWithSubjectPredicate(Node, property).FirstOrDefault();
        }
        return null;
    }

    public IResource getObject(INode property)
    {
        Triple t = getProperty(property);
        if (t != null)
        {
            return Resource.Get(t.Object, Graph, _model);
        }
        else
        {
            return null;
        }
    }

    public IResource getResource(INode property)
    {
        IResource obj = getObject(property);
        if (obj != null && !obj.isLiteral())
        {
            return obj;
        }
        return null;
    }

    public ILiteralNode getLiteral(INode property)
    {
        IResource obj = getObject(property);
        if (obj != null && obj.isLiteral())
        {
            return (ILiteralNode)obj.getSource();
        }
        return null;
    }

    public bool? getBoolean(INode property)
    {
        ILiteralNode obj = getLiteral(property);
        if (obj is IValuedNode valuedNode)
        {
            return valuedNode.AsBoolean();
        }
        return null;
    }

    public int? getInteger(INode property)
    {
        ILiteralNode obj = getLiteral(property);
        if (obj is IValuedNode valuedNode)
        {
            return (int)valuedNode.AsInteger();
        }
        return null;
    }
    public long? getLong(INode property)
    {
        ILiteralNode obj = getLiteral(property);
        if (obj is IValuedNode valuedNode)
        {
            return valuedNode.AsInteger();
        }
        return null;
    }

    public string getString(INode property)
    {
        ILiteralNode obj = getLiteral(property);
        return obj?.Value;
    }


    public IResource inferRDFNode(INode property)
    {
        IResource existing = getObject(property);
        return existing;
    }

    #endregion

    public int CompareTo(IResource other)
    {
        return other == null ? 1 : Node.CompareTo(other.getSource());
    }

    public bool Equals(IResource other)
    {
        if (other == null) return false;
        return Node.Equals(other.getSource());
    }
}