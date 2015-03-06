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
    // Replace the GetXXX function with properties
    public class SpinStatistics
    {

        private INode _context;

        private TimeSpan _duration;

        private String _label;

        private String _queryText;

        private DateTime _startTime;

        private int _resultCount = -1;

        /**
         * Creates a new SPINStatistics object.
         * @param label  the label of the action that has been measured
         * @param queryText  the text of the query that was executed
         * @param duration  the total duration in ms
         * @param startTime  the start time of execution (for ordering)
         * @param context  the INode that for example was holding the spin:rule
         */
        public SpinStatistics(String label, String queryText, TimeSpan duration, DateTime startTime, INode context)
            :this(label, queryText, duration, startTime, context, -1)
        { 
        }

        public SpinStatistics(String label, String queryText, TimeSpan duration, DateTime startTime, INode context, int resultCount)
        {
            this._context = context;
            this._duration = duration;
            this._label = label;
            this._queryText = queryText;
            this._startTime = startTime;
            this._resultCount = resultCount;
        }

        public INode Context
        {
            get
            {
                return _context;
            }
        }


        public TimeSpan Duration
        {
            get
            {
                return _duration;
            }
        }


        public String Label
        {
            get
            {
                return _label;
            }
        }


        public String QueryText
        {
            get
            {
                return _queryText;
            }
        }

        public int ResultCount
        {
            get
            {
                return _resultCount;
            }
        }

        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
        }
    }
}
