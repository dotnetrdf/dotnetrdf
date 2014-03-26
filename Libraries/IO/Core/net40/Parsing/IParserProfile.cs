using System;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Interface for parser profiles
    /// </summary>
    /// <remarks>
    /// <para>
    /// The parser profile provides per-run configuration to a parser, this is to be used in addition to any instance level configuration for the parser.
    /// </para>
    /// <para>
    /// This interface is intentionally very lightweight right now, in future the interface may be extended to allow passing more per-run configuration to parsers.
    /// </para>
    /// </remarks>
    public interface IParserProfile
    {
        /// <summary>
        /// Gets initial namespaces used for parsing
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default parsers usually start with zero namespaces defined and may discover namespaces as they parse the data.  In some cases it may be desireable to provide initial mappings in order to work around buggy data.
        /// </para>
        /// </remarks>
        INamespaceMapper Namespaces { get; }

        /// <summary>
        /// Gets initial Base URI
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default parsers start with no Base URI and this may then change over time as they parse the data.  In some cases it may be desireable to provide an initial Base URI in order to work around buggy data or to have sensible resolution of relative URIs in the data.
        /// </para>
        /// </remarks>
        Uri BaseUri { get; }

        /// <summary>
        /// Gets the blank node generator to use
        /// </summary>
        IBlankNodeGenerator BlankNodeGenerator { get; }
    }
}
