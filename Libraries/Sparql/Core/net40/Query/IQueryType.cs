namespace VDS.RDF.Query
{
    /// <summary>
    /// Types of SPARQL Query
    /// </summary>
    public enum QueryType : int
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Ask
        /// </summary>
        Ask = 1,
        /// <summary>
        /// Constuct
        /// </summary>
        Construct = 2,
        /// <summary>
        /// Describe
        /// </summary>
        Describe = 3,
        /// <summary>
        /// Describe All
        /// </summary>
        DescribeAll = 4,
        /// <summary>
        /// Select
        /// </summary>
        Select = 5,
        /// <summary>
        /// Select Distinct
        /// </summary>
        SelectDistinct = 6,
        /// <summary>
        /// Select Reduced
        /// </summary>
        SelectReduced = 7,
        /// <summary>
        /// Select All
        /// </summary>
        SelectAll = 8,
        /// <summary>
        /// Select All Distinct
        /// </summary>
        SelectAllDistinct = 9,
        /// <summary>
        /// Select All Reduced
        /// </summary>
        SelectAllReduced = 10
    }
}
