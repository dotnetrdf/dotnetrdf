/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
namespace VDS.RDF.Query.Spin.Model
{
    /**
     * Jena wrapper for spl:Attribute.
     * 
     * @author Holger Knublauch
     */
    public interface IAttribute : IAbstractAttribute
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