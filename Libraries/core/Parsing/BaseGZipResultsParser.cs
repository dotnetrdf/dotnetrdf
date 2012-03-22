/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.IO.Compression;
using System.IO;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    public abstract class BaseGZipResultsParser
        : ISparqlResultsReader
    {
        private ISparqlResultsReader _parser;

        public BaseGZipResultsParser(ISparqlResultsReader parser)
        {
            if (parser == null) throw new ArgumentNullException("parser");
            this._parser = parser;
        }

        public void Load(SparqlResultSet results, StreamReader input)
        {
            if (results == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Set");
            this.Load(new ResultSetHandler(results), input);
        }

        public void Load(SparqlResultSet results, TextReader input)
        {
            if (results == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Set");
            this.Load(new ResultSetHandler(results), input);
        }

        public void Load(SparqlResultSet results, string filename)
        {
            if (results == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Set");
            this.Load(new ResultSetHandler(results), filename);
        }

        public void Load(ISparqlResultsHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot parse SPARQL Results using a null Handler");
            if (input == null) throw new RdfParseException("Cannot parse SPARQL Results from a null input");

            if (input.BaseStream is GZipStream)
            {
                this._parser.Load(handler, input);
            }
            else
            {
                //Force the inner stream to be GZipped
                input = new StreamReader(new GZipStream(input.BaseStream, CompressionMode.Decompress));
                this._parser.Load(handler, input);
            }
        }

        public void Load(ISparqlResultsHandler handler, TextReader input)
        {
            if (input is StreamReader)
            {
                this.Load(handler, (StreamReader)input);
            }
            else
            {
                throw new RdfParseException("GZipped input can only be parsed from StreamReader instances");
            }
        }

        public void Load(ISparqlResultsHandler handler, string filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse SPARQL Results from a null file");
            this.Load(handler, new StreamReader(new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)));
        }

        public override string ToString()
        {
            return "GZipped " + this._parser.ToString();
        }

        public event SparqlWarning Warning;
    }

    public class GZippedSparqlXmlParser
        : BaseGZipResultsParser
    {
        public GZippedSparqlXmlParser()
            : base(new SparqlXmlParser()) { }
    }

    public class GZippedSparqlJsonParser
        : BaseGZipResultsParser
    {
        public GZippedSparqlJsonParser()
            : base(new SparqlJsonParser()) { }
    }

    public class GZippedSparqlCsvParser
        : BaseGZipResultsParser
    {
        public GZippedSparqlCsvParser()
            : base(new SparqlCsvParser()) { }
    }

    public class GZippedSparqlTsvParser
        : BaseGZipResultsParser
    {
        public GZippedSparqlTsvParser()
            : base(new SparqlTsvParser()) { }
    }
}
