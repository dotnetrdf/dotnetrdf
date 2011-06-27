using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Linq
{
    public interface ILinqResultsSink
    {
        void Fill(SparqlResultSet results);

        void Fill(BaseMultiset multiset);
    }

    public abstract class BaseLinqResultsSink : ILinqResultsSink
    {

        public void Fill(SparqlResultSet results)
        {
            foreach (SparqlResult result in results)
            {
                this.ProcessResult(result);
            }
        }

        public void Fill(BaseMultiset multiset)
        {
            foreach (ISet s in multiset.Sets)
            {
                this.ProcessResult(new SparqlResult(s));
            }
        }

        protected abstract void ProcessResult(SparqlResult result);
    }
}
