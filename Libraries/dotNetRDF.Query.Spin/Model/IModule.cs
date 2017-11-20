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

using VDS.RDF;
using System.Collections.Generic;
using System;
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * Instances of spin:Module (or subclasses thereof).
     * 
     * @author Holger Knublauch
     */
    internal interface IModule : IResource
    {

        /**
         * Gets a List of all declared Arguments.
         * If ordered, then the local names of the predicates are used.
         * @param ordered  true to get an ordered list back (slower)
         * @return the (possibly empty) List of Arguments
         */
        List<IArgument> getArguments(bool ordered);


        /**
         * Gets a Map of variable names to Arguments.
         * @return a Map of variable names to Arguments
         */
        Dictionary<String, IArgument> getArgumentsMap();


        /**
         * Gets the body (if defined).  The result will be type cast into the
         * most specific subclass of Command if possible.
         * @return the body or null
         */
        ICommand getBody();


        /**
         * Gets the rdfs:comment of this (if any).
         * @return the comment or null
         */
        String getComment();


        /**
         * Checks if this Module has been declared to be abstract using spin:abstract.
         * @return true  if this is abstract
         */
        bool isAbstract();
    }
}