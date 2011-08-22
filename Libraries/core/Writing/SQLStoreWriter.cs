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

#if !NO_DATA && !NO_STORAGE

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
    /// Class for storing entire Triple Stores into SQL backed Storage
    /// </summary>
    /// <remarks>Uses single threaded writing which will be very slow for large Triple Stores, use a <see cref="ThreadedSqlStoreWriter">ThreadedSqlStoreWriter</see> for large Stores</remarks>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public class SqlStoreWriter : IStoreWriter
    {
        /// <summary>
        /// Saves the given Triple Store to the SQL Store that this class was instantiated with
        /// </summary>
        /// <param name="store">Store you wish to Save</param>
        /// <param name="parameters">Parameters for the Store</param>
        public virtual void Save(ITripleStore store, IStoreParams parameters)
        {
            if (parameters is ISQLIOParams)
            {
                SqlIOParams writeParams = (SqlIOParams)parameters;
                writeParams.Manager.PreserveState = true;
                SqlWriter writer = new SqlWriter(writeParams.Manager);

                foreach (IGraph g in store.Graphs)
                {
                    writer.Save(g, writeParams.ClearIfExists);
                }
            }
            else
            {
                throw new RdfStorageException("Parameters for the SQLStoreWriter must implement the interface ISQLIOParams");
            }
        }

        /// <summary>
        /// Event which is raised when there is an issue with the Graphs being serialized that doesn't prevent serialization but the user should be aware of
        /// </summary>
        public virtual event StoreWriterWarning Warning;
    }

    /// <summary>
    /// Class for storing entire Triple Stores into SQL backed Storage which uses multi-threaded writing for improved write speeds
    /// </summary>
    public class ThreadedSqlStoreWriter : SqlStoreWriter
    {
        /// <summary>
        /// Saves the given Triple Store to the Target Store that this class was instantiated with
        /// </summary>
        /// <param name="store">Store to Save</param>
        /// <param name="parameters">Parameters for the Store</param>
        public override void Save(ITripleStore store, IStoreParams parameters)
        {
            if (parameters is ThreadedSqlIOParams)
            {
                //Setup context
                ThreadedSqlStoreWriterContext context = new ThreadedSqlStoreWriterContext(store, (ThreadedSqlIOParams)parameters);
                bool transSetting = context.Manager.DisableTransactions;
                context.Manager.DisableTransactions = true;

                //Queue Graphs for Writing
                foreach (IGraph g in store.Graphs)
                {
                    context.Add(g);
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

                //Reset Transactions setting
                context.Manager.DisableTransactions = transSetting;

                //If there were any errors we'll throw an RdfThreadedOutputException now
                if (outputEx.InnerExceptions.Any()) throw outputEx;

            }
            else
            {
                throw new RdfStorageException("Parameters for the ThreadedSQLStoreWriter must be of type ThreadedSQLIOParams");
            }
        }

        private delegate void WriteGraphsDelegate(ThreadedSqlStoreWriterContext context);

        /// <summary>
        /// Internal Worker method for Writer Threads
        /// </summary>
        private void WriteGraphs(ThreadedSqlStoreWriterContext context)
        {
            try
            {
                int thread = Thread.CurrentThread.ManagedThreadId;
                SqlWriter writer = new SqlWriter(context.Manager);

                IGraph g = context.GetNextGraph();
                while (g != null)
                {
                    //Write the Graph to the Target Store
                    writer.Save(g, context.ClearIfExists);

                    //Get Next Graph
                    g = context.GetNextGraph();
                } 
            }
            catch (ThreadAbortException)
            {
                //We've been terminated, don't do anything
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Error in Threaded Writer in Thread ID " + Thread.CurrentThread.ManagedThreadId, ex);
            }
        }

        /// <summary>
        /// Event which is raised when there is an issue with the Graphs being serialized that doesn't prevent serialization but the user should be aware of
        /// </summary>
        public override event StoreWriterWarning Warning;

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