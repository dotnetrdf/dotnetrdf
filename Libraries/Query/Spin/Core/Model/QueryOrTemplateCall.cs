/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;

namespace VDS.RDF.Query.Spin.Model
{


    /**
     * A wrapper of either a Query or a TemplateCall.
     * 
     * @author Holger Knublauch
     */
    public class QueryOrTemplateCall
    {

        private IResource cls;

        private IQueryResource query;

        private ITemplateCallResource templateCall;


        /**
         * Constructs an instance representing a plain Query.
         * @param cls  the class the query is attached to
         * @param query  the SPIN Query
         */
        public QueryOrTemplateCall(IResource cls, IQueryResource query)
        {
            this.cls = cls;
            this.query = query;
        }


        /**
         * Constructs an instance representing a template call.
         * @param cls  the class the template call is attached to
         * @param templateCall  the template call
         */
        public QueryOrTemplateCall(IResource cls, ITemplateCallResource templateCall)
        {
            this.cls = cls;
            this.templateCall = templateCall;
        }


        /**
         * If this is a Query, then get it.
         * @return the Query or null
         */
        public IQueryResource getQuery()
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
        public ITemplateCallResource getTemplateCall()
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