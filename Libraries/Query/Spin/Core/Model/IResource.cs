/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{

    /**
     * An extension of the INode interface with additional
     * convenience methods to easier access property values. 
     * 
     * @author Holger Knublauch
     */
    public interface IResource : INode
    {

        INode AsNode();
        SpinModel GetModel();

        bool IsUri();
        bool IsBlank();
        bool IsLiteral();

        String UniqueIdentifier { get; }
        Uri Uri { get; }

        bool CanAs(INode cls);
        IResource As(Type cls);

        void AddProperty(INode predicate, INode value);
        bool HasProperty(INode property);
        bool HasProperty(INode property, INode value);

        IEnumerable<Triple> ListProperties();
        IEnumerable<Triple> ListProperties(INode property);
        Triple GetProperty(INode property);

        IEnumerable<IResource> GetObjects(INode property);
        List<IResource> AsList();

        IResource GetObject(INode predicate);
        IResource GetResource(INode predicate);

        ILiteralNode GetLiteral(INode predicate);
        bool? GetBoolean(INode predicate);
        int? GetInteger(INode predicate);
        long? GetLong(INode predicate);
        String GetString(INode predicate);
    }

    public class ModeledResourceComparer
        : IEqualityComparer<IResource>
    {
        public bool Equals(IResource x, IResource y)
        {
            if (x == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(IResource obj)
        {
            return obj.GetHashCode();
        }
    }
}