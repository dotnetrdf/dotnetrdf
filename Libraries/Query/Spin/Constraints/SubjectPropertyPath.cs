/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;

namespace VDS.RDF.Query.Spin.Constraints
{

    /**
     * A SimplePropertyPath of the form OP->S.
     * 
     * @author Holger Knublauch
     */
    public class SubjectPropertyPath : SimplePropertyPath
    {

        public SubjectPropertyPath(INode obj, INode predicate)
            : base(obj, predicate)
        {

        }
    }
}