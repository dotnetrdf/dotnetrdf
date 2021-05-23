using VDS.RDF.Update;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for Triple Stores which support SPARQL Update as per the SPARQL 1.1 specifications.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A Store which supports this may implement various access control mechanisms which limit what operations are actually permitted.
    /// </para>
    /// <para>
    /// It is the responsibility of the Store class to ensure that commands are permissible before invoking them.
    /// </para>
    /// </remarks>
    public interface ISparqlUpdateTripleStore : IUpdateableTripleStore
    {
        /// <summary>
        /// Executes a single Update Command against the Triple Store.
        /// </summary>
        /// <param name="update">SPARQL Update Command.</param>
        void ExecuteUpdate(SparqlUpdateCommand update);

        /// <summary>
        /// Executes a set of Update Commands against the Triple Store.
        /// </summary>
        /// <param name="updates">SPARQL Update Command Set.</param>
        void ExecuteUpdate(SparqlUpdateCommandSet updates);
    }
}
