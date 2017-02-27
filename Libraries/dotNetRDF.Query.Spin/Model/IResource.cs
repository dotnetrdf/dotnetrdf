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

        INode getSource();
        SpinProcessor getModel();

        bool isUri();
        bool isBlank();
        bool isLiteral();

        Uri Uri
        {
            get;
        }

        bool canAs(INode cls);
        IResource As(Type cls);

        void AddProperty(INode predicate, INode value);
        bool hasProperty(INode property);
        bool hasProperty(INode property, INode value);

        IEnumerable<Triple> listProperties();
        IEnumerable<Triple> listProperties(INode property);
        Triple getProperty(INode property);

        IEnumerable<IResource> getObjects(INode property);
        List<IResource> AsList();

        IResource getObject(INode predicate);
        IResource getResource(INode predicate);

        ILiteralNode getLiteral(INode predicate);
        bool? getBoolean(INode predicate);
        int? getInteger(INode predicate);
        long? getLong(INode predicate);
        String getString(INode predicate);
    }
}