/*

Copyright Robert Vesse 2009-10
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
using System.IO;
using System.Linq;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// A Class for parsing RDF data from Data URIs
    /// </summary>
    /// <remarks>
    /// <para>
    /// Data URIs use the data: scheme and are defined by the IETF in <a href="http://tools.ietf.org/html/rfc2397">RFC 2397</a> and provide a means to embed data directly in a URI either in Base64 or ASCII encoded format.  This class can extract the data from such URIs and attempt to parse it as RDF using the <see cref="StringParser">StringParser</see>
    /// </para>
    /// <para>
    /// The parsing process for data: URIs involves first extracting and decoding the data embedded in the URI - this may either be in Base64 or ASCII encoding - and then using the <see cref="StringParser">StringParser</see> to actually parse the data string.  If the data: URI defines a MIME type then a parser is selected (if one exists for the given MIME type) and that is used to parse the data, in the event that no MIME type is given or the one given does not have a corresponding parser then the <see cref="StringParser">StringParser</see> will use its basic heuristics to attempt to auto-detect the format and select an appropriate parser.
    /// </para>
    /// <para>
    /// If you attempt to use this loader for non data: URIs then the standard <see cref="UriLoader">UriLoader</see> is used instead.
    /// </para>
    /// </remarks>
    public static class DataUriLoader
    {
        /// <summary>
        /// Loads RDF data into a Graph from a data: URI
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="u">URI to load from</param>
        /// <remarks>
        /// Invokes the normal <see cref="UriLoader">UriLoader</see> instead if a the URI provided is not a data: URI
        /// </remarks>
        /// <exception cref="UriFormatException">Thrown if the metadata portion of the URI which indicates the MIME Type, Character Set and whether Base64 encoding is used is malformed</exception>
        public static void Load(IGraph g, Uri u)
        {
            if (u == null) throw new RdfParseException("Cannot load RDF from a null URI");
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            DataUriLoader.Load(new GraphHandler(g), u);
        }

        /// <summary>
        /// Loads RDF data using an RDF Handler from a data: URI
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="u">URI to load from</param>
        /// <remarks>
        /// Invokes the normal <see cref="UriLoader">UriLoader</see> instead if a the URI provided is not a data: URI
        /// </remarks>
        /// <exception cref="UriFormatException">Thrown if the metadata portion of the URI which indicates the MIME Type, Character Set and whether Base64 encoding is used is malformed</exception>
        public static void Load(IRdfHandler handler, Uri u)
        {
            if (u == null) throw new RdfParseException("Cannot load RDF from a null URI");
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");

            if (!u.Scheme.Equals("data"))
            {
                //Invoke the normal URI Loader if not a data: URI
#if !SILVERLIGHT
                UriLoader.Load(handler, u);
#else
                UriLoader.Load(handler, u, (_,_1) => { }, null);
#endif
                return;
            }

            String mimetype = "text/plain";
            bool mimeSet = false;
            bool base64 = false;
            String[] uri = u.AbsolutePath.Split(',');
            String metadata = uri[0];
            String data = uri[1];

            //Process the metadata
            if (metadata.Equals(String.Empty))
            {
                //Nothing to do
            }
            else if (metadata.Contains(';'))
            {
                if (metadata.StartsWith(";")) metadata = metadata.Substring(1);
                String[] meta = metadata.Split(';');
                for (int i = 0; i < meta.Length; i++)
                {
                    if (meta[i].StartsWith("charset="))
                    {
                        //OPT: Do we need to process the charset parameter here at all?
                        //String charset = meta[i].Substring(meta[i].IndexOf('=') + 1);
                    }
                    else if (meta[i].Equals("base64"))
                    {
                        base64 = true;
                    }
                    else if (meta[i].Contains('/'))
                    {
                        mimetype = meta[i];
                        mimeSet = true;
                    }
                    else
                    {
                        throw new UriFormatException("This data: URI appears to be malformed as encountered the parameter value '" + meta[i] + "' in the metadata section of the URI");
                    }
                }
            }
            else
            {
                if (metadata.StartsWith("charset="))
                {
                    //OPT: Do we need to process the charset parameter here at all?
                }
                else if (metadata.Equals(";base64"))
                {
                    base64 = true;
                }
                else if (metadata.Contains('/'))
                {
                    mimetype = metadata;
                    mimeSet = true;
                }
                else
                {
                    throw new UriFormatException("This data: URI appears to be malformed as encountered the parameter value '" + metadata + "' in the metadata section of the URI");
                }
            }

            //Process the data
            if (base64)
            {
                StringWriter temp = new StringWriter();
                foreach (byte b in Convert.FromBase64String(data))
                {
                    temp.Write((char)((int)b));
                }
                data = temp.ToString();
            }
            else
            {
                data = Uri.UnescapeDataString(data);
            }

            //Now either select a parser based on the MIME type or let StringParser guess the format
            try
            {
                if (mimeSet)
                {
                    //If the MIME Type was explicitly set then we'll try and get a parser and use it
                    IRdfReader reader = MimeTypesHelper.GetParser(mimetype);
                    reader.Load(handler, new StringReader(data));
                }
                else
                {
                    //If the MIME Type was not explicitly set we'll let the StringParser guess the format
                    IRdfReader reader = StringParser.GetParser(data);
                    reader.Load(handler, new StringReader(data));
                }
            }
            catch (RdfParserSelectionException)
            {
                //If we fail to get a parser then we'll let the StringParser guess the format
                IRdfReader reader = StringParser.GetParser(data);
                reader.Load(handler, new StringReader(data));
            }
        }
    }
}
