/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;

namespace VDS.RDF.Query.Spin.Statistics
{

    /**
     * A wrapper to record the execution time of a given Query
     * for statistical purposes.
     * 
     * @author Holger Knublauch
     */

    public class SPINStatistics
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
