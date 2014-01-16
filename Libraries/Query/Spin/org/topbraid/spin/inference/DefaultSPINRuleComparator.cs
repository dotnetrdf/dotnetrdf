/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using org.topbraid.spin.model;
using org.topbraid.spin.util;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;

namespace org.topbraid.spin.inference
{

    /**
     * A SPINRuleComparator using the spin:nextRuleProperty property.
     *
     * @author Holger Knublauch
     */
    public class DefaultSPINRuleComparer : ISPINRuleComparer
    {

        private List<IResource> properties;


        public DefaultSPINRuleComparer(Model model)
        {
            // Pre-build properties list
            properties = (List<IResource>)model.GetAllSubProperties(SPIN.rule, true);
        }


        public int Compare(CommandWrapper w1, CommandWrapper w2)
        {
            if (properties.Count() > 1)
            {
                IResource p1 = Resource.Get(w1.getStatement() != null ? w1.getStatement().Predicate : SPIN.rule);
                IResource p2 = Resource.Get(w2.getStatement() != null ? w2.getStatement().Predicate : SPIN.rule);
                if (!p1.Equals(p2))
                {
                    int index1 = properties.IndexOf(p1);
                    int index2 = properties.IndexOf(p2);
                    int compare = index1.CompareTo(index2);
                    if (compare != 0)
                    {
                        return compare;
                    }
                }
            }
            return w1.getText().CompareTo(w2.getText());
        }
    }
}