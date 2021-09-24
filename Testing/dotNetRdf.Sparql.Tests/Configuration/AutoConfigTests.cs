using Xunit;
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.DateTime;
using VDS.RDF.Nodes;

namespace VDS.RDF.Configuration
{
    public class AutoConfigTests
    {
        public AutoConfigTests()
        {
            ConfigurationLoader.RegisterExtension<SparqlConfigurationLoaderExtension>();
        }

        [Fact]
        public void ConfigurationAutoOperators1()
        {
            try
            {
                var data = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:SparqlOperator ;
dnr:type """ + typeof(MockSparqlOperator).AssemblyQualifiedName + @""" .";

                var g = new Graph();
                g.LoadFromString(data);
                ConfigurationLoader.AutoConfigure(g);

                SparqlOperators.TryGetOperator(SparqlOperatorType.Add, false, out var op, null);

                Assert.Equal(typeof(MockSparqlOperator), op.GetType());
                SparqlOperators.RemoveOperator(op);
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }

        [Fact]
        public void ConfigurationAutoOperators2()
        {
            try
            {
                var data = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:SparqlOperator ;
dnr:type ""VDS.RDF.Query.Operators.DateTime.DateTimeAddition"" ;
dnr:enabled false .";

                var g = new Graph();
                g.LoadFromString(data);
                ConfigurationLoader.AutoConfigure(g);

                Assert.False(SparqlOperators.IsRegistered(new DateTimeAddition()));
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }
    }

    public class MockSparqlOperator
        : ISparqlOperator
    {

        #region ISparqlOperator Members

        public SparqlOperatorType Operator => SparqlOperatorType.Add;

        public bool IsExtension => true;

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