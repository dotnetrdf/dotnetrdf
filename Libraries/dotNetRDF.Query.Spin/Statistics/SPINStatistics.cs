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

namespace VDS.RDF.Query.Spin.Statistics
{

    /**
     * A wrapper to record the execution time of a given Query
     * for statistical purposes.
     * 
     * @author Holger Knublauch
     */

    internal class SPINStatistics
    {

        private INode _context;

        private TimeSpan _duration;

        private String _label;

        private String _queryText;

        private DateTime _startTime;


        /**
         * Creates a new SPINStatistics object.
         * @param label  the label of the action that has been measured
         * @param queryText  the text of the query that was executed
         * @param duration  the total duration in ms
         * @param startTime  the start time of execution (for ordering)
         * @param context  the INode that for example was holding the spin:rule
         */
        public SPINStatistics(String label, String queryText, TimeSpan duration, DateTime startTime, INode context)
        {
            this._context = context;
            this._duration = duration;
            this._label = label;
            this._queryText = queryText;
            this._startTime = startTime;
        }


        public INode getContext()
        {
            return _context;
        }


        public TimeSpan getDuration()
        {
            return _duration;
        }


        public String getLabel()
        {
            return _label;
        }


        public String getQueryText()
        {
            return _queryText;
        }


        public DateTime getStartTime()
        {
            return _startTime;
        }
    }
}
