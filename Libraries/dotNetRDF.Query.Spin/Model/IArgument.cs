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
using System;

namespace VDS.RDF.Query.Spin.Model
{

    /**
     * Jena wrapper for instances of spl:Argument.
     * 
     * @author Holger Knublauch
     */
    internal interface IArgument : IAbstractAttribute
    {


        /**
         * If this is an ordered arg (sp:arg1, sp:arg2, ...) then this returns
         * the index of this, otherwise null.
         * @return the arg index or null if this does not have an index
         */
        int? getArgIndex();


        /**
         * Returns any declared spl:defaultValue.
         * @return the default value or null
         */
        INode getDefaultValue();


        /**
         * Gets the variable name associated with this Argument.
         * This is the local name of the predicate, i.e. implementations
         * can assume that this value is not null iff getPredicate() != null.
         * @return the variable name
         */
        String getVarName();
    }
}