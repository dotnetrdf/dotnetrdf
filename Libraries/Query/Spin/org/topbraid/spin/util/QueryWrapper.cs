using org.topbraid.spin.model;
using VDS.RDF;
using System;
using VDS.RDF.Query;
namespace org.topbraid.spin.util
{

    /**
     * A CommandWrapper that wraps a SPARQL query 
     * (in contrast to UpdateWrapper for UPDATE requests).
     * 
     * @author Holger Knublauch
     */
    public class QueryWrapper : CommandWrapper
    {

        private SparqlParameterizedString query;

        private org.topbraid.spin.model.IQuery spinQuery;


        public QueryWrapper(SparqlParameterizedString query, INode source, String text, IQuery spinQuery, String label, Triple statement, bool thisUnbound, int thisDepth)
            : base(source, text, label, statement, thisUnbound, thisDepth)
        {
            
            this.query = query;
            this.spinQuery = spinQuery;
        }


        public SparqlParameterizedString getQuery()
        {
            return query;
        }


        override public ICommand getSPINCommand()
        {
            return getSPINQuery();
        }


        public org.topbraid.spin.model.IQuery getSPINQuery()
        {
            return spinQuery;
        }
    }
}