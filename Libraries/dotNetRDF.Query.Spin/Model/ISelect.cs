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
namespace VDS.RDF.Query.Spin.Model
{


    /**
     * A SELECT query.
     * 
     * @author Holger Knublauch
     */
    internal interface ISelect : ISolutionModifierQuery
    {

        /**
         * Gets a list of result variables, or null if we have a star
         * results list.  Note that the "variables" may in fact be
         * wrapped aggregations or expressions.
         * The results can be tested with is against
         * <code>Variable</code>, <code>Aggregation</code> or
         * <code>FunctionCall</code>.  Variables can have an additional
         * <code>sp:expression</code>, representing AS expressions.
         * @return the result "variables"
         */
        List<IResource> getResultVariables();


        /**
         * Checks is this query has the DISTINCT flag set.
         * @return true if distinct
         */
        bool isDistinct();


        /**
         * Checks if this query has the REDUCED flag set.
         * @return true if reduced
         */
        bool isReduced();
    }
}