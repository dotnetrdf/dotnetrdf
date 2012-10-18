using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Algebra
{
    public class Table
        : ITerminalOperator
    {
        private BaseMultiset _table;

        public Table(BaseMultiset table)
        {
            if (table == null) throw new ArgumentNullException("table");
            this._table = table;
        }

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context.OutputMultiset = this._table;
            return context.OutputMultiset;
        }

        public IEnumerable<string> Variables
        {
            get
            {
                return this._table.Variables; 
            }
        }

        public SparqlQuery ToQuery()
        {
            throw new NotSupportedException("Cannot convert Table to Query");
        }

        public Patterns.GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("Cannot convert Table to Graph Pattern");
        }

        public override string ToString()
        {
            return "Table()";
        }
    }
}
