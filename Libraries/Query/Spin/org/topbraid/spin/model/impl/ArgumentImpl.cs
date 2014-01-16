/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{
    public class ArgumentImpl : AbstractAttributeImpl, IArgument
    {

        public ArgumentImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public int? getArgIndex()
        {
            String varName = getVarName();
            if (varName != null)
            {
                return SP.getArgPropertyIndex(varName);
            }
            else
            {
                return null;
            }
        }


        public INode getDefaultValue()
        {
            return getObject(SPL.PropertyDefaultValue);
        }


        public String getVarName()
        {
            IResource argProperty = getPredicate();
            if (argProperty != null)
            {
                return argProperty.Uri().ToString().Replace(SP.BASE_URI, "");
            }
            else
            {
                return null;
            }
        }


        public bool IsOptional()
        {
            return (bool)getBoolean(SPL.PropertyOptional);
        }
    }
}