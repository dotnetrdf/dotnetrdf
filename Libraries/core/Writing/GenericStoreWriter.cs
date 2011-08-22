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
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for writing Triple Stores to an arbitrary store using any <see cref="IGenericIOManager">IGenericIOManager</see> implementation
    /// </summary>
    [Obsolete("The GenericStoreWriter is considered obsolete - use the SaveGraph() method of an IGenericIOManager directly instead", false)]
    public class GenericStoreWriter : IStoreWriter
    {
        /// <summary>
        /// Saves the given Triple Store to an arbitrary store
        /// </summary>
        /// <param name="store">Store to Save</param>
        /// <param name="parameters">Parameters for the Store</param>
        /// <remarks>
        /// Parameters must be of type <see cref="GenericIOParams">GenericIOParams</see>
        /// </remarks>
        public void Save(ITripleStore store, IStoreParams parameters)
        {
            if (parameters is GenericIOParams)
            {
                //Create the Writer Context
                GenericStoreWriterContext context = new GenericStoreWriterContext(store, (GenericIOParams)parameters);

                //Queue Graphs for Writing
                foreach (IGraph g in store.Graphs)
                {
                    context.Add(g.BaseUri);
                }
                
                //Start making the async calls
                List<IAsyncResult> results = new List<IAsyncResult>();
                WriteGraphsDelegate d = new WriteGraphsDelegate(this.WriteGraphs);
                for (int i = 0; i < context.Threads; i++)
                {
                    results.Add(d.BeginInvoke(context, null, null));
                }

                //Wait for all the async calls to complete
                WaitHandle.WaitAll(results.Select(r => r.AsyncWaitHandle).ToArray());
                RdfThreadedOutputException outputEx = new RdfThreadedOutputException(WriterErrorMessages.ThreadedOutputFailure("TSV"));
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
                throw new RdfStorageException("Parameters for the GenericStoreWriter must be of type GenericIOParams");
            }
        }

        /// <summary>
        /// Delegate for the WriteGraphs method
        /// </summary>
        /// <param name="context">Context for writing the Store</param>
        private delegate void WriteGraphsDelegate(GenericStoreWriterContext context);

        /// <summary>
        /// Internal Worker method for Writer Threads
        /// </summary>
        /// <param name="context">Context for writing the Store</param>
        private void WriteGraphs(GenericStoreWriterContext context)
        {
            try
            {
                Uri u = context.GetNextURI();
                while (u != null)
                {
                    //Get the Graph from the Store
                    IGraph g = context.Store.Graph(u);

                    //Write the Graph to the Target Store
                    context.Manager.SaveGraph(g);

                    //Get Next Graph Uri
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
        /// Event which is raised if there is a non-fatal issue with writing to the underlying Store
        /// </summary>
        public event StoreWriterWarning Warning;
    }
}

#endif