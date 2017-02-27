/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;

namespace VDS.RDF.Query.Spin.Constraints
{


    /**
     * A SimplePropertyPath of the form SP->O.
     * 
     * @author Holger Knublauch
     */
    public class ObjectPropertyPath : SimplePropertyPath
    {

        public ObjectPropertyPath(INode subject, INode predicate)
            : base(subject, predicate)
        {
        }
    }
}