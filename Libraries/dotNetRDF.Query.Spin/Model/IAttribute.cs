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
namespace VDS.RDF.Query.Spin.Model
{
    /**
     * Jena wrapper for spl:Attribute.
     * 
     * @author Holger Knublauch
     */
    internal interface IAttribute : IAbstractAttribute
    {

        /**
         * Gets the declared default value of this attribute, as defined
         * using spl:defaultValue.  Might be null.
         * @return the default value
         */
        INode getDefaultValue();


        /**
         * Gets the maximum cardinality of this attribute, if specified.
         * This is based on spl:maxCount.  Null if unspecified.
         * @return the maximum cardinality or null if none is given
         */
        int? getMaxCount();


        /**
         * Gets the minimum cardinality of this attribute.
         * This is based on spl:minCount.  Default value is 0.
         * @return the minimum cardinality
         */
        int getMinCount();
    }
}