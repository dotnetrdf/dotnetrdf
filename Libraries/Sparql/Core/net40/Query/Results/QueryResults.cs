using VDS.RDF.Graphs;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Basic implementation of query results
    /// </summary>
    public class QueryResults
        : IQueryResults
    {
        public QueryResults(bool result)
        {
            this.IsBoolean = true;
            this.IsGraph = false;
            this.IsTabular = false;
            this.Boolean = result;
        }

        public QueryResults(ITabularResults results)
        {
            this.IsBoolean = false;
            this.IsGraph = false;
            this.IsTabular = true;
            this.Table = results;
        }

        public QueryResults(IGraph g)
        {
            this.IsBoolean = false;
            this.IsGraph = true;
            this.IsTabular = false;
            this.Graph = g;
        }

        public bool IsTabular { get; private set; }
        public bool IsGraph { get; private set; }
        public bool IsBoolean { get; private set; }
        public ITabularResults Table { get; private set; }
        public IGraph Graph { get; private set; }
        public bool? Boolean { get; private set; }
    }
}