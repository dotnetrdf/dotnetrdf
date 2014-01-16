/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using org.topbraid.spin.model;
using VDS.RDF;

namespace org.topbraid.spin.util
{


    /**
     * Wraps a (pre-compiled) SPARQL Query or Update with its source SPIN object and
     * a human-readable string representation. 
     * 
     * Also needed to work around the issue of Query.equals/hashCode: Otherwise
     * multiple distinct template calls will be merged into one in HashMaps.
     * 
     * @author Holger Knublauch
     */
    public abstract class CommandWrapper
    {

        private String label;

        private INode source;

        private Triple statement;

        // Used to store the arguments if this wraps a Template call
        private Dictionary<String, IResource> templateBinding;

        private String text;

        private int? thisDepth;

        private bool thisUnbound;


        public CommandWrapper(INode source, String text, String label, Triple statement, bool thisUnbound, int? thisDepth)
        {
            this.label = label;
            this.statement = statement;
            this.source = source;
            this.text = text;
            this.thisDepth = thisDepth;
            this.thisUnbound = thisUnbound;
        }


        public Dictionary<String, IResource> getTemplateBinding()
        {
            return templateBinding;
        }


        public String getLabel()
        {
            return label;
        }


        public abstract ICommand getSPINCommand();


        public Triple getStatement()
        {
            return statement;
        }


        /**
         * Gets the SPIN Query or template call that has created this QueryWrapper. 
         * @return the source
         */
        public INode getSource()
        {
            return source;
        }


        public String getText()
        {
            return text;
        }


        /**
         * Gets the maximum depth of ?this in the element tree.
         * May be null if either not computed (?thisUnbound) or ?this does not exist.
         * @return the max depth of ?this or null
         */
        public int? getThisDepth()
        {
            return thisDepth;
        }


        public bool isThisDeep()
        {
            return thisDepth != null && thisDepth > 1;
        }


        public bool isThisUnbound()
        {
            return thisUnbound;
        }


        public void setTemplateBinding(Dictionary<String, IResource> value)
        {
            this.templateBinding = value;
        }
    }
}