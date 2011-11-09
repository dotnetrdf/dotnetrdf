/*

Copyright Robert Vesse 2009-11
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper Class for loading RDF Files into Graphs/Triple Stores
    /// </summary>
    public static class FileLoader
    {
        /// <summary>
        /// Loads the contents of the given File into a Graph providing the RDF format can be determined
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="filename">File to load from</param>
        /// <remarks>
        /// <para>
        /// The <see cref="FileLoader">FileLoader</see> first attempts to select a RDF Parser by examining the file extension to select the most likely MIME type for the file.  This assumes that the file extension corresponds to one of the recognized file extensions for a RDF format the library supports.  If this suceeds then a parser is chosen and will be used to attempt to parse the input.
        /// </para>
        /// <para>
        /// Should this fail then the contents of the file will be read into a String, the <see cref="StringParser">StringParser</see> is then used to attempt to parse it.  The <see cref="StringParser">StringParser</see> uses some simple rules to guess which format the input is likely to be and chooses a parser based on it's guess.
        /// </para>
        /// <para>
        /// <strong>Note:</strong> FileLoader will assign the Graph a file Uri prior to attempting Parsing, any Base Uri specified in the RDF contained in the file will override this Uri.
        /// </para>
        /// <para>
        /// The File Uri assigned will always be an absolute Uri for the File
        /// </para>
        /// </remarks>
        public static void Load(IGraph g, String filename)
        {
            FileLoader.Load(g, filename, null);
        }

        /// <summary>
        /// Loads the contents of the given File into a Graph using the given RDF Parser
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="filename">File to load from</param>
        /// <param name="parser">Parser to use</param>
        public static void Load(IGraph g, String filename, IRdfReader parser)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");

            if (!File.Exists(filename))
            {
#if SILVERLIGHT
                throw new FileNotFoundException("Cannot read RDF from the File '" + filename + "' since it doesn't exist");
#else
                throw new FileNotFoundException("Cannot read RDF from a File that doesn't exist", filename);
#endif
            }

            //Assign a File Uri to the Graph if the Graph is Empty
            //It's possible that when we parse in the RDF this may be changed but it ensures the Graph 
            //has a Base Uri even if the RDF doesn't specify one
            //Ensure that the Uri is an absolute file Uri
            if (g.IsEmpty && g.BaseUri == null)
            {
                RaiseWarning("Assigned a file: URI as the Base URI for the input Graph");
                if (Path.IsPathRooted(filename))
                {
                    g.BaseUri = new Uri("file:///" + filename);
                }
                else
                {
                    g.BaseUri = new Uri("file:///" + Path.GetFullPath(filename));
                }
            }

            FileLoader.Load(new GraphHandler(g), filename, parser);
        }

        /// <summary>
        /// Loads the contents of the given File using a RDF Handler providing the RDF format can be determined
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to load from</param>
        /// <remarks>
        /// <para>
        /// The <see cref="FileLoader">FileLoader</see> first attempts to select a RDF Parser by examining the file extension to select the most likely MIME type for the file.  This assumes that the file extension corresponds to one of the recognized file extensions for a RDF format the library supports.  If this suceeds then a parser is chosen and will be used to attempt to parse the input.
        /// </para>
        /// <para>
        /// Should this fail then the contents of the file will be read into a String, the <see cref="StringParser">StringParser</see> is then used to attempt to parse it.  The <see cref="StringParser">StringParser</see> uses some simple rules to guess which format the input is likely to be and chooses a parser based on it's guess.
        /// </para>
        /// <para>
        /// <strong>Note:</strong> FileLoader will assign the Graph a file Uri prior to attempting Parsing, any Base Uri specified in the RDF contained in the file will override this Uri.
        /// </para>
        /// <para>
        /// The File Uri assigned will always be an absolute Uri for the File
        /// </para>
        /// </remarks>
        public static void Load(IRdfHandler handler, String filename)
        {
            FileLoader.Load(handler, filename, (IRdfReader)null);
        }

        /// <summary>
        /// Loads the contents of the given File using a RDF Handler using the given RDF Parser
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to load from</param>
        /// <param name="parser">Parser to use</param>
        public static void Load(IRdfHandler handler, String filename, IRdfReader parser)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF using a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");

            //Try to get a Parser from the File Extension if one isn't explicitly specified
            if (parser == null)
            {
                try
                {
                    parser = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeTypes(Path.GetExtension(filename)));
                    RaiseWarning("Selected Parser " + parser.ToString() + " based on file extension, if this is incorrect consider specifying the parser explicitly");
                }
                catch (RdfParserSelectionException)
                {
                    //If error then we couldn't determine MIME Type from the File Extension
                    RaiseWarning("Unable to select a parser by determining MIME Type from the File Extension");
                }
            }

            if (parser == null)
            {
                //Unable to determine format from File Extension
                //Read file in locally and use the StringParser to select a parser
                RaiseWarning("Attempting to select parser based on analysis of the data file, this requires loading the file into memory");
                StreamReader reader = new StreamReader(filename);
                String data = reader.ReadToEnd();
                reader.Close();
                parser = StringParser.GetParser(data);
                RaiseWarning("Used the StringParser to guess the parser to use - it guessed " + parser.GetType().Name);
                parser.Warning += RaiseWarning;
                parser.Load(handler, new StringReader(data));
            }
            else
            {
                //Parser was selected based on File Extension or one was explicitly specified
                parser.Warning += RaiseWarning;
                parser.Load(handler, filename);
            }
        }

        /// <summary>
        /// Loads the contents of the given File into a Triple Store providing the RDF dataset format can be determined
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="filename">File to load from</param>
        /// <param name="parser">Parser to use to parse the given file</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then the <see cref="FileLoader">FileLoader</see> attempts to select a Store Parser by examining the file extension to select the most likely MIME type for the file.  This assume that the file extension corresponds to one of the recognized file extensions for a RDF dataset format the library supports.  If this suceeds then a parser is chosen and used to parse the input file.
        /// </para>
        /// </remarks>
        public static void Load(ITripleStore store, String filename, IStoreReader parser)
        {
            if (store == null) throw new RdfParseException("Cannot read a RDF Dataset into a null Store");
            FileLoader.Load(new StoreHandler(store), filename, parser);
        }

        /// <summary>
        /// Loads the contents of the given File into a Triple Store providing the RDF dataset format can be determined
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="filename">File to load from</param>
        /// <remarks>
        /// <para>
        /// The <see cref="FileLoader">FileLoader</see> attempts to select a Store Parser by examining the file extension to select the most likely MIME type for the file.  This assume that the file extension corresponds to one of the recognized file extensions for a RDF dataset format the library supports.  If this suceeds then a parser is chosen and used to parse the input file.
        /// </para>
        /// </remarks>
        public static void Load(ITripleStore store, String filename)
        {
            FileLoader.Load(store, filename, null);
        }

        /// <summary>
        /// Loads the contents of the given File using a RDF Handler providing the RDF dataset format can be determined
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to load from</param>
        /// <param name="parser">Parser to use to parse the given file</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then the <see cref="FileLoader">FileLoader</see> attempts to select a Store Parser by examining the file extension to select the most likely MIME type for the file.  This assume that the file extension corresponds to one of the recognized file extensions for a RDF dataset format the library supports.  If this suceeds then a parser is chosen and used to parse the input file.
        /// </para>
        /// </remarks>
        public static void Load(IRdfHandler handler, String filename, IStoreReader parser)
        {
            if (handler == null) throw new RdfParseException("Cannot read a RDF Dataset using a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read a RDF Dataset from a null File");

            if (!File.Exists(filename))
            {
#if SILVERLIGHT
                throw new FileNotFoundException("Cannot read a RDF Dataset from the File '" + filename + "' since it doesn't exist");
#else
                throw new FileNotFoundException("Cannot read a RDF Dataset from a File that doesn't exist", filename);
#endif
            }

            if (parser == null)
            {
                try
                {
                    parser = MimeTypesHelper.GetStoreParser(MimeTypesHelper.GetMimeType(Path.GetExtension(filename)));
                }
                catch (RdfParserSelectionException)
                {
                    //If error then we couldn't determine MIME Type from the File Extension
                    RaiseWarning("Unable to select a parser by determining MIME Type from the File Extension");
                }
            }
            if (parser == null)
            {
                //Unable to determine format from File Extension
                //Read file in locally and use the StringParser to select a parser
                StreamReader reader = new StreamReader(filename);
                String data = reader.ReadToEnd();
                reader.Close();
                parser = StringParser.GetDatasetParser(data);
                RaiseWarning("Used the StringParser to guess the parser to use - it guessed " + parser.GetType().Name);
                parser.Warning += RaiseStoreWarning;
                parser.Load(handler, new TextReaderParams(new StringReader(data)));
            }
            else
            {
                parser.Warning += RaiseStoreWarning;
                parser.Load(handler, new StreamParams(filename));
            }
        }

        /// <summary>
        /// Loads the contents of the given File using a RDF Handler providing the RDF dataset format can be determined
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to load from</param>
        /// <remarks>
        /// <para>
        /// The <see cref="FileLoader">FileLoader</see> attempts to select a Store Parser by examining the file extension to select the most likely MIME type for the file.  This assume that the file extension corresponds to one of the recognized file extensions for a RDF dataset format the library supports.  If this suceeds then a parser is chosen and used to parse the input file.
        /// </para>
        /// </remarks>
        public static void LoadDataset(IRdfHandler handler, String filename)
        {
            FileLoader.Load(handler, filename, (IStoreReader)null);
        }

        /// <summary>
        /// Raises warning messages
        /// </summary>
        /// <param name="message">Warning Message</param>
        static void RaiseWarning(String message)
        {
            RdfReaderWarning d = Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Raises Store Warning messages
        /// </summary>
        /// <param name="message">Warning Message</param>
        static void RaiseStoreWarning(String message)
        {
            StoreReaderWarning d = StoreWarning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which is raised when the parser invoked by the FileLoader detects a non-fatal issue with the RDF syntax
        /// </summary>
        public static event RdfReaderWarning Warning;

        /// <summary>
        /// Event which is raised when the Store parser invoked by the FileLoader detects a non-fatal issue with the RDF syntax
        /// </summary>
        public static event StoreReaderWarning StoreWarning;
    }
}
