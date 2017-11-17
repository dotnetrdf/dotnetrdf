/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
    internal interface IResource : INode
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