/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{

    public class AttributeImpl : AbstractAttributeImpl, IAttribute
    {

        public AttributeImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public bool IsOptional()
        {
            return getMinCount() == 0;
        }


        public INode getDefaultValue()
        {
            return getObject(SPL.PropertyDefaultValue);
        }


        public int? getMaxCount()
        {
            int? value = (int?)getLong(SPL.PropertyMaxCount);
            if (value != null)
            {
                return value;
            }
            else
            {
                return null;
            }
        }


        public int getMinCount()
        {
            int? value = (int?)getLong(SPL.PropertyMaxCount);
            if (value != null)
            {
                return (int)value;
            }
            else
            {
                return 0;
            }
        }
    }
}