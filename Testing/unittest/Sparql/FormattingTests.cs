using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class FormattingTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        [TestMethod]
        public void SparqlFormattingOptionalAtRoot()
        {
            SparqlQuery q = new SparqlQuery { QueryType = SparqlQueryType.Select };
            q.AddVariable(new SparqlVariable("s", true));

            GraphPattern gp = new GraphPattern();
            gp.IsOptional = true;
            gp.AddTriplePattern(new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o")));
            q.RootGraphPattern = gp;

            String toStr = q.ToString();
            Console.WriteLine("ToString() Form:");
            Console.WriteLine(toStr);
            Assert.AreEqual(2, toStr.ToCharArray().Where(c => c == '{').Count());
            Assert.AreEqual(2, toStr.ToCharArray().Where(c => c == '}').Count());
            Console.WriteLine();

            SparqlFormatter formatter = new SparqlFormatter();
            String fmtStr = formatter.Format(q);
            Console.WriteLine("SparqlFormatter Form:");
            Console.WriteLine(fmtStr);
            Assert.AreEqual(2, fmtStr.ToCharArray().Where(c => c == '{').Count());
            Assert.AreEqual(2, fmtStr.ToCharArray().Where(c => c == '}').Count());
        }

    }
}
