namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Node Type Values
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// A Blank Node
        /// </summary>
        Blank = 0, 
        /// <summary>
        /// A Uri Node
        /// </summary>
        Uri = 1, 
        /// <summary>
        /// A Literal Node
        /// </summary>
        Literal = 2,
        /// <summary>
        /// A Graph Literal Node
        /// </summary>
        GraphLiteral = 3,
        /// <summary>
        /// A Variable Node
        /// </summary>
        Variable = 4
    }
}