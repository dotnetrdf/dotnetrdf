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

using System.Collections.Generic;
using System;
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * Base interface of the various SPARQL query types such as
     * Ask, Construct, Describe and Select.
     * 
     * @author Holger Knublauch
     */
    internal interface IQuery : ICommandWithWhere
    {

        /**
         * Gets the list of URIs specified in FROM clauses.
         * @return a List of URI Strings
         */
        List<IResource> getFrom();


        /**
         * Gets the list of URIs specified in FROM NAMED clauses.
         * @return a List of URI Strings
         */
        List<IResource> getFromNamed();


        /**
         * Gets the VALUES block at the end of the query if it exists. 
         * @return the Values or null
         */
        IValues getValues();


        /**
         * Gets the elements in the WHERE clause of this query.
         * The Elements will be typecast into the best suitable subclass.
         * @return a List of Elements
         */
        List<IElement> getWhereElements();
    }
}