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

using VDS.RDF.Storage;
using System.Collections.Generic;
using VDS.RDF;
using System;
using VDS.RDF.Query.Spin.SparqlUtil;
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * Part of a SPARQL expression that calls a Function.
     * 
     * @author Holger Knublauch
     */
    internal interface IFunctionCall : IPrintable, IModuleCall
    {

        /**
         * Gets a list of argument RDFNodes, whereby each INode is already cast
         * into the most specific subclass possible.  In particular, arguments are
         * either instances of Variable, FunctionCall or INode (constant)
         * @return the List of arguments
         */
        List<IResource> getArguments();


        /**
         * Gets a Map from properties (such as sp:arg1, sp:arg2) to their declared
         * argument values.  The map will only contain non-null arguments.
         * @return a Map of arguments
         */
        Dictionary<IResource, IResource> getArgumentsMap();


        /**
         * Gets the URI INode of the Function being called here.
         * The resulting INode will be in the function's defining
         * Model, for example if loaded into the library from a .spin. file.
         * @return the function in its original Model
         */
        IResource getFunction();
    }
}