namespace VDS.RDF.Query.Spin.SparqlUtil
{
    internal interface ISparqlSDPlugin
    {
        INode Resource { get; }

        IGraph SparqlSDContribution { get; }
    }
}