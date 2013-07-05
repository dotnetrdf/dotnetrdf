using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static helper class for loading data from a stream into Graphs / Triple Stores
    /// </summary>
    public static class StreamLoader
    {
        public static void Load(IGraph g, string filename, Stream inputStream)
        {
            Load(g, filename, inputStream, null);
        }

        public static void Load(IGraph g, string filename, Stream inputStream, IRdfReader parser)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (inputStream == null) throw new RdfParseException("Cannot read RDF from a null Stream");
            if (g.IsEmpty && g.BaseUri == null)
            {
                throw new RdfParseException("Cannot read RDF into an empty Graph with no BaseUri");
            }

            Load(new GraphHandler(g), filename, inputStream, parser);
        }

        public static void Load(IRdfHandler handler, string filename, Stream inputStream)
        {
            Load(handler, filename, inputStream, null);
        }

        public static void Load(IRdfHandler handler, string filename, Stream inputStream, IRdfReader parser)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF using a null RDF Handler");
            if (inputStream == null) throw new RdfParseException("Cannot read RDF from a null Stream");

            if (parser == null)
            {
                if (filename != null)
                {
                    try
                    {
                        String ext = MimeTypesHelper.GetTrueFileExtension(filename);
                        parser = MimeTypesHelper.GetParserByFileExtension(ext);
                    }
                    catch (RdfParserSelectionException)
                    {
                        //If error then we couldn't determine MIME Type from the File Extension
                        RaiseWarning("Unable to select a parser by determining MIME Type from the File Extension");
                    }
                }
            }

            if (parser == null)
            {
                //Unable to determine format from File Extension
                //Read file in locally and use the StringParser to select a parser
                RaiseWarning("Attempting to select parser based on analysis of the data file, this requires loading the file into memory");
                StreamReader reader = new StreamReader(inputStream);
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
                parser.Load(handler, new StreamReader(inputStream));
            }

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
