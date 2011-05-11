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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Class for reading Triple Stores that are saved on disk as a set of files in a Folder
    /// </summary>
    //[Obsolete("This class is deprecated in favour of using the Alexandria Filesystem store provided by the dotNetRDF.Alexandria library", false)]
    public class FolderStoreReader : IStoreReader
    {
        /// <summary>
        /// Loads Graphs into the store using the settings the Reader was instantiated with
        /// </summary>
        /// <param name="store">Store to load into</param>
        /// <param name="parameters">Parameters indicating where to read from</param>
        public void Load(ITripleStore store, IStoreParams parameters)
        {
            if (store == null) throw new RdfParseException("Cannot read RDF from a Folder Store into a null Triple Store");
            this.Load(new StoreHandler(store), parameters);
        }

        /// <summary>
        /// Loads RDF using the RDF Handler using the settings the Reader was instantiated with
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="parameters">Parameters indicating where to read from</param>
        public void Load(IRdfHandler handler, IStoreParams parameters)
        {
            if (parameters is FolderStoreParams)
            {
                //Create the Parser Context
                FolderStoreParserContext context = new FolderStoreParserContext(handler, (FolderStoreParams)parameters);

                //Create the Folder
                if (!Directory.Exists(context.Folder))
                {
                    throw new RdfStorageException("Cannot read a Folder Store from a Folder that doesn't exist");
                }

                //Read list of Graphs and Queue Filenames of Graphs for reading by the Threads
                StreamReader graphlist = new StreamReader(Path.Combine(context.Folder, "graphs.fstore"));

                //First line contains format information
                String ext = graphlist.ReadLine();
                if (context.Format == FolderStoreFormat.AutoDetect)
                {
                    if (ext.Equals(".ttl"))
                    {
                        context.Format = FolderStoreFormat.Turtle;
                    }
                    else if (ext.Equals(".n3"))
                    {
                        context.Format = FolderStoreFormat.Notation3;
                    }
                    else if (ext.Equals(".rdf"))
                    {
                        context.Format = FolderStoreFormat.RdfXml;
                    }
                    else
                    {
                        throw new RdfStorageException("Folder Store Format auto-detection failed");
                    }
                }

                String file;
                while (!graphlist.EndOfStream)
                {
                    file = graphlist.ReadLine();
                    if (!file.Equals(String.Empty))
                    {
                        context.Add(file);
                    }
                }
                graphlist.Close();

                //Start making the async calls
                List<IAsyncResult> results = new List<IAsyncResult>();
                LoadGraphsDelegate d = new LoadGraphsDelegate(this.LoadGraphs);
                for (int i = 0; i < context.Threads; i++)
                {
                    results.Add(d.BeginInvoke(context, null, null));
                }

                //Wait for all the async calls to complete
                WaitHandle.WaitAll(results.Select(r => r.AsyncWaitHandle).ToArray());
                RdfThreadedParsingException parsingEx = new RdfThreadedParsingException("One/more errors occurred while parsing RDF from a Folder Store using a multi-threaded parsing process");
                foreach (IAsyncResult result in results)
                {
                    try
                    {
                        d.EndInvoke(result);
                    }
                    catch (Exception ex)
                    {
                        parsingEx.AddException(ex);
                    }
                }

                //If there were any errors we'll throw an RdfThreadedOutputException now
                if (parsingEx.InnerExceptions.Any()) throw parsingEx;
            }
            else
            {
                throw new RdfStorageException("Parameters for the FolderStoreReader must be of type FolderStoreParams");
            }
        }

        /// <summary>
        /// Delegate for LoadGraphs method
        /// </summary>
        private delegate void LoadGraphsDelegate(FolderStoreParserContext context);

        /// <summary>
        /// Internal Method which performs multi-threaded reading of data
        /// </summary>
        private void LoadGraphs(FolderStoreParserContext context)
        {
            //Create the relevant Parser
            IRdfReader parser;
            switch (context.Format)
            {
                case FolderStoreFormat.Turtle:
                    parser = new TurtleParser();
                    break;
                case FolderStoreFormat.Notation3:
                    parser = new Notation3Parser();
                    break;
                case FolderStoreFormat.RdfXml:
                    parser = new RdfXmlParser();
                    break;
                default:
                    parser = new TurtleParser();
                    break;
            }

            try
            {
                String file = context.GetNextFilename();
                while (file != null)
                {
                    //Read from Disk
                    Graph g = new Graph();
                    String sourceFile = Path.Combine(context.Folder, file);
                    parser.Load(g, sourceFile);

                    //Add to Graph Collection
                    foreach (Triple t in g.Triples)
                    {
                        if (context.Terminated) break;
                        if (!context.Handler.HandleTriple(t)) ParserHelper.Stop();
                    }

                    if (context.Terminated) break;

                    //Get the Next Filename
                    file = context.GetNextFilename();
                }
            }
            catch (ThreadAbortException)
            {
                //We've been terminated, don't do anything
#if !SILVERLIGHT
                Thread.ResetAbort();
#endif
            }
            catch (RdfParsingTerminatedException)
            {
                context.Terminated = true;
                context.ClearFilenames();
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Error in Threaded Reader in Thread ID " + Thread.CurrentThread.ManagedThreadId, ex);
            }
        }

        /// <summary>
        /// Helper method used to raise the Warning event if there is an event handler registered
        /// </summary>
        /// <param name="message">Warning message</param>
        private void RaiseWarning(String message)
        {
            StoreReaderWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        public event StoreReaderWarning Warning;
    }
}

#endif