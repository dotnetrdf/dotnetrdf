using VDS.RDF.Nodes;
using VDS.RDF.Query.Operators;

namespace VDS.RDF.Configuration
{
    public class MockSparqlOperator
        : ISparqlOperator
    {

        #region ISparqlOperator Members

        public SparqlOperatorType Operator
        {
            get { return SparqlOperatorType.Add; }
        }

        public bool IsApplicable(params IValuedNode[] ns)
        {
            return true;
        }

        public IValuedNode Apply(params Nodes.IValuedNode[] ns)
        {
            return null;
        }

        #endregion
    }

}
