using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF
{
    public class SparqlMimeTypeDefinition
        : MimeTypeDefinition
    {
        /// <summary>
        /// Creates a new MIME Type Definition
        /// </summary>
        /// <param name="syntaxName">Syntax Name for the Syntax which has this MIME Type definition</param>
        /// <param name="mimeTypes">MIME Types</param>
        /// <param name="fileExtensions">File Extensions</param>
        /// <param name="rdfParserType">Type to use to parse RDF (or null if not applicable)</param>
        /// <param name="sparqlResultsParserType">Type to use to parse SPARQL Results (or null if not applicable)</param>
        /// <param name="rdfWriterType">Type to use to writer RDF (or null if not applicable)</param>
        /// <param name="sparqlResultsWriterType">Type to use to write SPARQL Results (or null if not applicable)</param>
        public SparqlMimeTypeDefinition(String syntaxName, IEnumerable<String> mimeTypes, IEnumerable<String> fileExtensions, Type rdfParserType, Type sparqlResultsParserType, Type rdfWriterType, Type sparqlResultsWriterType)
            : base(syntaxName, mimeTypes, fileExtensions, rdfParserType, rdfWriterType)
        {
            this.SparqlResultsParserType = sparqlResultsParserType;
            this.SparqlResultsWriterType = sparqlResultsWriterType;
        }

        /// <summary>
        /// Creates a new MIME Type Definition
        /// </summary>
        /// <param name="syntaxName">Syntax Name for the Syntax which has this MIME Type definition</param>
        /// <param name="formatUri">Format URI as defined by the <a href="http://www.w3.org/ns/formats/">W3C</a></param>
        /// <param name="mimeTypes">MIME Types</param>
        /// <param name="fileExtensions">File Extensions</param>
        /// <param name="rdfParserType">Type to use to parse RDF (or null if not applicable)</param>
        /// <param name="sparqlResultsParserType">Type to use to parse SPARQL Results (or null if not applicable)</param>
        /// <param name="rdfWriterType">Type to use to writer RDF (or null if not applicable)</param>
        /// <param name="sparqlResultsWriterType">Type to use to write SPARQL Results (or null if not applicable)</param>
        public SparqlMimeTypeDefinition(String syntaxName, String formatUri, IEnumerable<String> mimeTypes, IEnumerable<String> fileExtensions, Type rdfParserType, Type sparqlResultsParserType, Type rdfWriterType, Type sparqlResultsWriterType)
            : this(syntaxName, mimeTypes, fileExtensions, rdfParserType, sparqlResultsParserType, rdfWriterType, sparqlResultsWriterType)
        {
            this._formatUri = formatUri;
        }

            /// <summary>
        /// Gets/Sets the Type to use to parse SPARQL Results (or null if not applicable)
        /// </summary>
        public Type SparqlResultsParserType
        {
            get
            {
                return this._sparqlResultsParserType;
            }
            set
            {
                if (value == null)
                {
                    this._sparqlResultsParserType = value;
                }
                else
                {
                    if (EnsureInterface("SPARQL Results Parser", value, typeof(ISparqlResultsReader)))
                    {
                        this._sparqlResultsParserType = value;
                    }
                }
            }
        }

    
        /// <summary>
        /// Gets/Sets the Type to use to write SPARQL Results (or null if not applicable)
        /// </summary>
        public Type SparqlResultsWriterType
        {
            get
            {
                return this._sparqlResultsWriterType;
            }
            set
            {
                if (value == null)
                {
                    this._sparqlResultsWriterType = value;
                }
                else
                {
                    if (EnsureInterface("SPARQL Results Writer", value, typeof(ISparqlResultsWriter)))
                    {
                        this._sparqlResultsWriterType = value;
                    }
                }
            }
        }

            /// <summary>
        /// Gets whether this definition can instantiate a Parser that can parse SPARQL Results
        /// </summary>
        public bool CanParseSparqlResults
        {
            get
            {
                return (this._sparqlResultsParserType != null);
            }
        }

    
        /// <summary>
        /// Gets whether the Definition provides a SPARQL Results Writer
        /// </summary>
        public bool CanWriteSparqlResults
        {
            get
            {
                return (this._sparqlResultsWriterType != null);
            }
        }

        /// <summary>
        /// Gets an instance of a SPARQL Results parser
        /// </summary>
        /// <returns></returns>
        public ISparqlResultsReader GetSparqlResultsParser()
        {
            if (this._sparqlResultsParserType != null)
            {
                return (ISparqlResultsReader)Activator.CreateInstance(this._sparqlResultsParserType);
            }
            else if (this._rdfParserType != null)
            {
                return new SparqlRdfParser((IRdfReader)Activator.CreateInstance(this._rdfParserType));
            }
            else
            {
                throw new RdfParserSelectionException("There is no SPARQL Results Parser available for the Syntax " + this._name);
            }
        }

        /// <summary>
        /// Gets an instance of a SPARQL Results writer
        /// </summary>
        /// <returns></returns>
        public ISparqlResultsWriter GetSparqlResultsWriter()
        {
            if (this._sparqlResultsWriterType != null)
            {
                return (ISparqlResultsWriter)Activator.CreateInstance(this._sparqlResultsWriterType);
            }
            else if (this._rdfWriterType != null)
            {
                return new SparqlRdfWriter((IRdfWriter)Activator.CreateInstance(this._rdfWriterType));
            }
            else
            {
                throw new RdfWriterSelectionException("There is no SPARQL Results Writer available for the Syntax " + this._name);
            }
        }
    }
}
