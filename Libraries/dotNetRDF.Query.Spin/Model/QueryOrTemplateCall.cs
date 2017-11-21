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

namespace VDS.RDF.Query.Spin.Model
{


    /**
     * A wrapper of either a Query or a TemplateCall.
     * 
     * @author Holger Knublauch
     */
    internal class QueryOrTemplateCall
    {

        private IResource cls;

        private IQuery query;

        private ITemplateCall templateCall;


        /**
         * Constructs an instance representing a plain Query.
         * @param cls  the class the query is attached to
         * @param query  the SPIN Query
         */
        public QueryOrTemplateCall(IResource cls, IQuery query)
        {
            this.cls = cls;
            this.query = query;
        }


        /**
         * Constructs an instance representing a template call.
         * @param cls  the class the template call is attached to
         * @param templateCall  the template call
         */
        public QueryOrTemplateCall(IResource cls, ITemplateCall templateCall)
        {
            this.cls = cls;
            this.templateCall = templateCall;
        }


        /**
         * If this is a Query, then get it.
         * @return the Query or null
         */
        public IQuery getQuery()
        {
            return query;
        }


        /**
         * Gets the associated subject, e.g. the rdfs:Class that holds the spin:rule. 
         * @return the subject
         */
        public IResource getCls()
        {
            return cls;
        }


        /**
         * If this is a TemplateCall, then return it.
         * @return the TemplateCall or null
         */
        public ITemplateCall getTemplateCall()
        {
            return templateCall;
        }


        /**
         * Gets a human-readable text of either the query or the template call.
         * Can also be used for sorting alphabetically.
         */
        public String toString()
        {
            if (getTemplateCall() != null)
            {
                return String.Empty; // SPINLabels.getLabel(getTemplateCall());
            }
            else
            {
                return String.Empty; //SPINLabels.getLabel(getQuery());
            }
        }
    }
}