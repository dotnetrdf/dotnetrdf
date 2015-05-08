/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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