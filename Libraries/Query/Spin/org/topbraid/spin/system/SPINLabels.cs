/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using org.topbraid.spin.model;
using VDS.RDF;
using VDS.RDF.Query.Spin;

namespace org.topbraid.spin.system
{

    /**
     * A singleton that is used to render resources into strings.
     * By default this displays qnames (if possible). 
     * Can be changed, for example, to switch to displaying rdfs:labels
     * instead of qnames etc.
     * 
     * @author Holger Knublauch
     */
    public class SPINLabels
    {

        /**
         * Gets a "human-readable" label for a given Resource.
         * This checks for any existing rdfs:label, otherwise falls back to
         * <code>getLabel()</code>.
         * @param resource
         * @return the label (never null)
         */
        public static String getCustomizedLabel(IResource resource)
        {
            String label = resource.getString(RDFS.PropertyLabel);
            if (label != null)
            {
                return label;
            }
            return getLabel(resource);
        }


        /**
         * Gets the label for a given Resource.
         * @param resource  the Resource to get the label of
         * @return the label (never null)
         */
        public static String getLabel(INode resource)
        {
            if (resource is IUriNode)
            {
                String qname = null; // TODO chercher dans les NS prefixes ;
                if (qname != null)
                {
                    return qname;
                }
                else
                {
                    return "<" + ((IUriNode)resource).Uri.ToString() + ">";
                }
            }
            else
            {
                return resource.ToString();
            }
        }
    }
}