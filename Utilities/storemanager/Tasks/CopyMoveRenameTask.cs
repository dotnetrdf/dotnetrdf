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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task for copying/moving graphs
    /// </summary>
    public class CopyMoveTask
        : CancellableTask<TaskResult>
    {
        private IStorageProvider _source, _target;
        private Uri _sourceUri, _targetUri;
        private CancellableHandler _canceller;

        /// <summary>
        /// Creates a new Copy/Move task
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="target">Target</param>
        /// <param name="sourceUri">Source URI</param>
        /// <param name="targetUri">Target URI</param>
        /// <param name="forceCopy">Whether to force a copy</param>
        public CopyMoveTask(IStorageProvider source, IStorageProvider target, Uri sourceUri, Uri targetUri, bool forceCopy)
            : base(GetName(source, target, sourceUri, targetUri, forceCopy))
        {
            this._source = source;
            this._target = target;
            this._sourceUri = sourceUri;
            this._targetUri = targetUri;
        }

        /// <summary>
        /// Gets the Source
        /// </summary>
        public IStorageProvider Source
        {
            get
            {
                return this._source;
            }
        }

        /// <summary>
        /// Gets the Target
        /// </summary>
        public IStorageProvider Target
        {
            get
            {
                return this._target;
            }
        }

        private static String GetName(IStorageProvider source, IStorageProvider target, Uri sourceUri, Uri targetUri, bool forceCopy)
        {
            if (ReferenceEquals(source, target) && !forceCopy)
            {
                //Source and Target Store are same so must be a Rename
                return "Move";
            }
            else
            {
                //Different Source and Target store so a Copy/Move
                if (forceCopy)
                {
                    //Source and Target URI are equal so a Copy
                    return "Copy";
                }
                else
                {
                    //Otherwise is a Move
                    return "Move";
                }
            }
        }

        /// <summary>
        /// Runs the task
        /// </summary>
        /// <returns></returns>
        protected override TaskResult RunTaskInternal()
        {
            if (this._target.IsReadOnly) throw new RdfStorageException("Cannot Copy/Move a Graph when the Target is a read-only Store!");

            switch (this.Name)
            {
                case "Move":
                    //Move a Graph 
                    if (ReferenceEquals(this._source, this._target) && this._source is IUpdateableStorage)
                    {
                        //If the Source and Target are identical and it supports SPARQL Update natively then we'll just issue a MOVE command
                        this.Information = "Issuing a MOVE command to renamed Graph '" + this._sourceUri.ToSafeString() + "' to '" + this._targetUri.ToSafeString() + "'";
                        SparqlParameterizedString update = new SparqlParameterizedString();
                        update.CommandText = "MOVE";
                        if (this._sourceUri == null)
                        {
                            update.CommandText += " DEFAULT TO";
                        }
                        else
                        {
                            update.CommandText += " GRAPH @source TO";
                            update.SetUri("source", this._sourceUri);
                        }
                        if (this._targetUri == null)
                        {
                            update.CommandText += " DEFAULT";
                        }
                        else
                        {
                            update.CommandText += " GRAPH @target";
                            update.SetUri("target", this._targetUri);
                        }
                        ((IUpdateableStorage)this._source).Update(update.ToString());
                        this.Information = "MOVE command completed OK, Graph renamed to '" + this._targetUri.AbsoluteUri + "'";
                    }
                    else
                    {
                        //Otherwise do a load of the source graph writing through to the target graph
                        IRdfHandler handler;
                        IGraph g = null;
                        if (this._target.UpdateSupported)
                        {
                            //If Target supports update then we'll use a WriteToStoreHandler combined with a GraphUriRewriteHandler
                            handler = new WriteToStoreHandler(this._target, this._targetUri);
                            handler = new GraphUriRewriteHandler(handler, this._targetUri);
                        }
                        else
                        {
                            //Otherwise we'll use a GraphHandler and do a save at the end
                            g = new Graph();
                            handler = new GraphHandler(g);
                        }
                        handler = new CopyMoveProgressHandler(handler, this, "Moving", this._target.UpdateSupported);
                        this._canceller = new CancellableHandler(handler);
                        if (this.HasBeenCancelled) this._canceller.Cancel();

                        //Now start reading out the data
                        this.Information = "Copying data from Graph '" + this._sourceUri.ToSafeString() + "' to '" + this._targetUri.ToSafeString() + "'";
                        this._source.LoadGraph(this._canceller, this._sourceUri);

                        //If we weren't moving the data directly need to save the resulting graph now
                        if (g != null)
                        {
                            this.Information = "Saving copied data to Target Store...";
                            this._target.SaveGraph(g);
                        }

                        //And finally since we've done a copy (not a move) so far we need to delete the original graph
                        //to effect a rename
                        if (this._source.DeleteSupported)
                        {
                            this.Information = "Removing source graph to complete the move operation";
                            this._source.DeleteGraph(this._sourceUri);

                            this.Information = "Move completed OK, Graph moved to '" + this._targetUri.ToSafeString() + "'" + (ReferenceEquals(this._source, this._target) ? String.Empty : " on " + this._target.ToString());
                        }
                        else
                        {
                            this.Information = "Copy completed OK, Graph copied to '" + this._targetUri.ToSafeString() + "'" + (ReferenceEquals(this._source, this._target) ? String.Empty : " on " + this._target.ToString()) + ".  Please note that as the Source Triple Store does not support deleting Graphs so the Graph remains present in the Source Store";
                        }
                    }

                    break;

                case "Copy":
                    if (ReferenceEquals(this._source, this._target) && this._source is IUpdateableStorage)
                    {
                        //If the Source and Target are identical and it supports SPARQL Update natively then we'll just issue a COPY command
                        this.Information = "Issuing a COPY command to copy Graph '" + this._sourceUri.ToSafeString() + "' to '" + this._targetUri.ToSafeString() + "'";
                        SparqlParameterizedString update = new SparqlParameterizedString();
                        update.CommandText = "COPY";
                        if (this._sourceUri == null)
                        {
                            update.CommandText += " DEFAULT TO";
                        }
                        else
                        {
                            update.CommandText += " GRAPH @source TO";
                            update.SetUri("source", this._sourceUri);
                        }
                        if (this._targetUri == null)
                        {
                            update.CommandText += " DEFAULT";
                        }
                        else
                        {
                            update.CommandText += " GRAPH @target";
                            update.SetUri("target", this._targetUri);
                        }
                        ((IUpdateableStorage)this._source).Update(update.ToString());
                        this.Information = "COPY command completed OK, Graph copied to '" + this._targetUri.ToSafeString() + "'";
                    }
                    else
                    {
                        //Otherwise do a load of the source graph writing through to the target graph
                        IRdfHandler handler;
                        IGraph g = null;
                        if (this._target.UpdateSupported)
                        {
                            //If Target supports update then we'll use a WriteToStoreHandler combined with a GraphUriRewriteHandler
                            handler = new WriteToStoreHandler(this._target, this._targetUri);
                            handler = new GraphUriRewriteHandler(handler, this._targetUri);
                        }
                        else
                        {
                            //Otherwise we'll use a GraphHandler and do a save at the end
                            g = new Graph();
                            handler = new GraphHandler(g);
                        }
                        handler = new CopyMoveProgressHandler(handler, this, "Copying", this._target.UpdateSupported);
                        this._canceller = new CancellableHandler(handler);
                        if (this.HasBeenCancelled) this._canceller.Cancel();

                        //Now start reading out the data
                        this.Information = "Copying data from Graph '" + this._sourceUri.ToSafeString() + "' to '" + this._targetUri.ToSafeString() + "'";
                        this._source.LoadGraph(this._canceller, this._sourceUri);

                        //If we weren't moving the data directly need to save the resulting graph now
                        if (g != null)
                        {
                            this.Information = "Saving copied data to Store...";
                            this._target.SaveGraph(g);
                        }

                        this.Information = "Copy completed OK, Graph copied to '" + this._targetUri.ToSafeString() + "'" + (ReferenceEquals(this._source, this._target) ? String.Empty : " on " + this._target.ToString());
                    }

                    break;
            }

            return new TaskResult(true);
        }

        /// <summary>
        /// Cancels the task
        /// </summary>
        protected override void CancelInternal()
        {
            if (this._canceller != null)
            {
                this._canceller.Cancel();
            }
        }
    }

    /// <summary>
    /// Handler for monitoring the progress of copy and moves
    /// </summary>
    class CopyMoveProgressHandler
        : BaseRdfHandler, IWrappingRdfHandler
    {
        private IRdfHandler _handler;
        private CopyMoveTask _task;
        private String _action;
        private bool _streaming;
        private int _count = 0;

        /// <summary>
        /// Creates a new handler
        /// </summary>
        /// <param name="handler">Handler</param>
        /// <param name="task">Task</param>
        /// <param name="action">Action</param>
        /// <param name="streaming">Whether the copy/move is streaming</param>
        public CopyMoveProgressHandler(IRdfHandler handler, CopyMoveTask task, String action, bool streaming)
        {
            this._handler = handler;
            this._task = task;
            this._action = action;
            this._streaming = streaming;
        }

        /// <summary>
        /// Returns that the handler accepts all triples
        /// </summary>
        public override bool AcceptsAll
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the inner handlers
        /// </summary>
        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handler.AsEnumerable();
            }
        }

        /// <summary>
        /// Starts handling RDF
        /// </summary>
        protected override void StartRdfInternal()
        {
            this._handler.StartRdf();
        }

        /// <summary>
        /// Ends RDF handling
        /// </summary>
        /// <param name="ok">Whether parsing finished OK</param>
        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
        }

        /// <summary>
        /// Handles Base URI declarations
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return this._handler.HandleBaseUri(baseUri);
        }

        /// <summary>
        /// Handles Namespace declarations
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return this._handler.HandleNamespace(prefix, namespaceUri);
        }

        /// <summary>
        /// Handles the triples
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            this._count++;
            if (this._count % 1000 == 0)
            {
                if (this._streaming)
                {
                    this._task.Information = this._action + " " + this._count + " triples so far...";
                }
                else
                {
                    this._task.Information = "Read " + this._count + " triples into memory so far, no " + this._action + " has yet taken place...";
                }
            }
            return this._handler.HandleTriple(t);
        }
    }
}
