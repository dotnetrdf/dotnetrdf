/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{


    public class SPINInstanceImpl : Resource, ISPINInstance
    {

        public SPINInstanceImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }

        // TODO relocate this into the SparqlWrapperDataset
        public List<QueryOrTemplateCall> getQueriesAndTemplateCalls(INode predicate)
        {
            List<QueryOrTemplateCall> results = new List<QueryOrTemplateCall>();
            //SparqlParameterizedString queryString = new SparqlParameterizedString("SELECT DISTINCT ?class WHERE {@instance a ?class}");
            //queryString.SetParameter("instance", getSource());
            //IEnumerator<SparqlResult> types = ((SparqlResultSet)getModel().Query(queryString.ToString())).Results.GetEnumerator();
            //for (; types.MoveNext(); )
            //{
            //    SPINUtil.addQueryOrTemplateCalls(Resource.Get(types.Current.Value("class"),getModel()), predicate, results);
            //}
            return results;
        }
    }
}