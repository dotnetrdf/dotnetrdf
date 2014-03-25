namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Possible NTriples syntax modes
    /// </summary>
    public enum NTriplesSyntax
    {
        /// <summary>
        /// The original NTriples syntax as specified in the original RDF specification <a href="http://www.w3.org/TR/2004/REC-rdf-testcases-20040210/">test cases</a> specification
        /// </summary>
        Original,

        /// <summary>
        /// Standardized NTriples as specified in the <a href="http://www.w3.org/TR/n-triples/">RDF 1.1 NTriples</a> specification
        /// </summary>
        Rdf11
    }
}