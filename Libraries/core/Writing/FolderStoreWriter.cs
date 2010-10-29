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
using System.Text;
using System.Threading;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{

    /// <summary>
    /// Class for storing entire Triple Stores into disk based storage as a set of files in a Folder
    /// </summary>
    [Obsolete("This class is deprecated in favour of using the Alexandria Filesystem store provided by the dotNetRDF.Alexandria library", false)]
    public class FolderStoreWriter : IStoreWriter
    {

        /// <summary>
        /// Saves the given Store to Disk using the settings the Writer was instantiated with
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="parameters">Parameters indicating where to save to</param>
        public void Save(ITripleStore store, IStoreParams parameters)
        {
            if (parameters is FolderStoreParams)
            {
                //Get the Parameters
                FolderStoreWriterContext context = new FolderStoreWriterContext(store, (FolderStoreParams)parameters);

                //Create the Folder
                if (!Directory.Exists(context.Folder))
                {
                    Directory.CreateDirectory(context.Folder);
                }

                //Write list of Graphs and Queue URIs of Graphs for writing by the async calls
                StreamWriter graphlist = new StreamWriter(Path.Combine(context.Folder,"graphs.fstore"));
                String ext;
                switch (context.Format)
                {
                    case FolderStoreFormat.Turtle:
                        ext = ".ttl";
                        break;
                    case FolderStoreFormat.Notation3:
                        ext = ".n3";
                        break;
                    case FolderStoreFormat.RdfXml:
                        ext = ".rdf";
                        break;
                    default:
                        ext = ".ttl";
                        break;
                }
                graphlist.WriteLine(ext);
                foreach (Uri u in context.Store.Graphs.GraphUris)
                {
                    context.Add(u);
                    graphlist.WriteLine(u.GetSha256Hash() + ext);
                }
                graphlist.Close();

                //Start making the async calls
                List<IAsyncResult> results = new List<IAsyncResult>();
                SaveGraphsDelegate d = new SaveGraphsDelegate(this.SaveGraphs);
                for (int i = 0; i < context.Threads; i++)
                {
                    results.Add(d.BeginInvoke(context, null, null));
                }

                //Wait for all the async calls to complete
                WaitHandle.WaitAll(results.Select(r => r.AsyncWaitHandle).ToArray());
                RdfThreadedOutputException outputEx = new RdfThreadedOutputException(WriterErrorMessages.ThreadedOutputFailure("Folder Store"));
                foreach (IAsyncResult result in results)
                {
                    try
                    {
                        d.EndInvoke(result);
                    }
                    catch (Exception ex)
                    {
                        outputEx.AddException(ex);
                    }
                }

                //If there were any errors we'll throw an RdfThreadedOutputException now
                if (outputEx.InnerExceptions.Any()) throw outputEx;
            }
            else
            {
                throw new RdfStorageException("Parameters for the FolderStoreWriter must be of type FolderStoreParams");
            }
        }

        /// <summary>
        /// Delegate for the SaveGraphs method
        /// </summary>
        /// <param name="context">Writer Context</param>
        private delegate void SaveGraphsDelegate(FolderStoreWriterContext context);

        /// <summary>
        /// Internal Method which performs multi-threaded writing of data
        /// </summary>
        private void SaveGraphs(FolderStoreWriterContext context)
        {
            //Create the relevant Writer
            IRdfWriter writer;
            switch (context.Format)
            {
                case FolderStoreFormat.Turtle:
                    writer = new TurtleWriter();
                    break;
                case FolderStoreFormat.Notation3:
                    writer = new Notation3Writer();
                    break;
                case FolderStoreFormat.RdfXml:
#if !NO_XMLDOM
                    writer = new FastRdfXmlWriter();
#else
                    writer = new RdfXmlWriter();
#endif
                    break;
                default:
                    writer = new TurtleWriter();
                    break;
            }

            try
            {
                Uri u = context.GetNextURI();
                while (u != null)
                {
                    //Get the Graph from the Store
                    IGraph g = context.Store.Graphs[u];

                    //Write to Disk
                    String destFile = Path.Combine(context.Folder, u.GetSha256Hash());
                    switch (context.Format)
                    {
                        case FolderStoreFormat.Turtle:
                            destFile += ".ttl";
                            break;
                        case FolderStoreFormat.Notation3:
                            destFile += ".n3";
                            break;
                        case FolderStoreFormat.RdfXml:
                            destFile += ".rdf";
                            break;
                        default:
                            destFile += ".ttl";
                            break;
                    }
                    writer.Save(g, destFile);

                    //Get the Next Uri
                    u = context.GetNextURI();
                }
            }
            catch (ThreadAbortException)
            {
                //We've been terminated, don't do anything
#if !SILVERLIGHT
                Thread.ResetAbort();
#endif
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Error in Threaded Writer in Thread ID " + Thread.CurrentThread.ManagedThreadId, ex);
            }
        }


        /// <summary>
        /// Event which is raised when there is an issue with the Graphs being serialized that doesn't prevent serialization but the user should be aware of
        /// </summary>
        public event StoreWriterWarning Warning;

        /// <summary>
        /// Internal Helper method which raises the Warning event only if there is an Event Handler registered
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void OnWarning(String message)
        {
            StoreWriterWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }
    }
}

#endif