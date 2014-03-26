using System;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Simple implementation of a parser profile
    /// </summary>
    public class ParserProfile
        : IParserProfile
    {
        /// <summary>
        /// Creates a new empty parser profile, this is the default used if no explicit profile is given
        /// </summary>
        public ParserProfile()
            : this(null, new NamespaceMapper(true)) {}

        /// <summary>
        /// Creates a new profile
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        public ParserProfile(Uri baseUri)
            : this(baseUri, null) {}

        /// <summary>
        /// Creates a new profile
        /// </summary>
        /// <param name="namespaces">Namespaces</param>
        public ParserProfile(INamespaceMapper namespaces)
            : this(null, namespaces) {}

        /// <summary>
        /// Creates a new profile
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <param name="namespaces">Namespaces</param>
        public ParserProfile(Uri baseUri, INamespaceMapper namespaces)
            : this(baseUri, namespaces, null) {}

        /// <summary>
        /// Creates a new profile
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <param name="namespaces">Namespaces</param>
        /// <param name="blankNodeGenerator">Blank Node Generator</param>
        public ParserProfile(Uri baseUri, INamespaceMapper namespaces, IBlankNodeGenerator blankNodeGenerator)
        {
            this.BaseUri = baseUri;
            this.Namespaces = namespaces ?? new NamespaceMapper(true);
            this.BlankNodeGenerator = blankNodeGenerator ?? new RandomDerivedBlankNodeGenerator();
        }

        /// <summary>
        /// Gets initial namespaces used for parsing
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default parsers usually start with zero namespaces defined and may discover namespaces as they parse the data.  In some cases it may be desireable to provide initial mappings in order to work around buggy data.
        /// </para>
        /// </remarks>
        public INamespaceMapper Namespaces { get; private set; }

        /// <summary>
        /// Gets initial Base URI
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default parsers start with no Base URI and this may then change over time as they parse the data.  In some cases it may be desireable to provide an initial Base URI in order to work around buggy data or to have sensible resolution of relative URIs in the data.
        /// </para>
        /// </remarks>
        public Uri BaseUri { get; private set; }

        /// <summary>
        /// Gets the blank node generator to use
        /// </summary>
        public IBlankNodeGenerator BlankNodeGenerator { get; private set; }
    }
}