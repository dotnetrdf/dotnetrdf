/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Utilities.StoreManager.Connections;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task for copying/moving graphs
    /// </summary>
    public class CopyMoveTask
        : CancellableTask<TaskResult>
    {
        private readonly Uri _sourceUri, _targetUri;
        private CancellableHandler _canceller;

        /// <summary>
        /// Creates a new Copy/Move task
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="target">Target</param>
        /// <param name="sourceUri">Source URI</param>
        /// <param name="targetUri">Target URI</param>
        /// <param name="forceCopy">Whether to force a copy</param>
        public CopyMoveTask(Connection source, Connection target, Uri sourceUri, Uri targetUri, bool forceCopy)
            : base(GetName(source, target, sourceUri, targetUri, forceCopy))
        {
            this.Source = source;
            this.Target = target;
            this._sourceUri = sourceUri;
            this._targetUri = targetUri;
        }

        /// <summary>
        /// Gets the Source
        /// </summary>
        public Connection Source { get; private set; }

        /// <summary>
        /// Gets the Target
        /// </summary>
        public Connection Target { get; private set; }

        private static String GetName(Connection source, Connection target, Uri sourceUri, Uri targetUri, bool forceCopy)
        {
            if (ReferenceEquals(source, target) && !forceCopy)
            {
                //Source and Target Store are same and not copying so must be a Rename
                return "Rename";
            }
            //Different Source and Target store so a Copy/Move
            return forceCopy ? "Copy" : "Move";
        }

        /// <summary>
        /// Runs the task
        /// </summary>
        /// <returns></returns>
        protected override TaskResult RunTaskInternal()
        {
            if (this.Target.IsReadOnly) throw new RdfStorageException("Cannot Copy/Move a Graph when the Target is a read-only Store!");

            switch (this.Name)
            {
                case "Rename":
                case "Move":
                    //Move/Rename a Graph 
                    if (ReferenceEquals(this.Source, this.Target) && this.Source.StorageProvider is IUpdateableStorage)
                    {
                        //If the Source and Target are identical and it supports SPARQL Update natively then we'll just issue a MOVE command
                        this.Information = "Issuing a MOVE command to rename Graph '" + this._sourceUri.ToSafeString() + "' to '" + this._targetUri.ToSafeString() + "'";
                        SparqlParameterizedString update = new SparqlParameterizedString {CommandText = "MOVE"};
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
                        ((IUpdateableStorage)this.Source.StorageProvider).Update(update.ToString());
                        this.Information = "MOVE command completed OK, Graph renamed to '" + this._targetUri.ToSafeString() + "'";
                    }
                    else
                    {
                        //Otherwise do a load of the source graph writing through to the target graph
                        IRdfHandler handler;
                        IGraph g = null;
                        if (this.Target.StorageProvider.UpdateSupported)
                        {
                            //If Target supports update then we'll use a WriteToStoreHandler combined with a GraphUriRewriteHandler
                            handler = new WriteToStoreHandler(this.Target.StorageProvider, this._targetUri);
                            handler = new GraphUriRewriteHandler(handler, this._targetUri);
                        }
                        else
                        {
                            //Otherwise we'll use a GraphHandler and do a save at the end
                            g = new Graph();
                            handler = new GraphHandler(g);
                        }
                        handler = new CopyMoveProgressHandler(handler, this, "Moving", this.Target.StorageProvider.UpdateSupported);
                        this._canceller = new CancellableHandler(handler);
                        if (this.HasBeenCancelled) this._canceller.Cancel();

                        //Now start reading out the data
                        this.Information = "Copying data from Graph '" + this._sourceUri.ToSafeString() + "' to '" + this._targetUri.ToSafeString() + "'";
                        this.Source.StorageProvider.LoadGraph(this._canceller, this._sourceUri);

                        //If we weren't moving the data directly need to save the resulting graph now
                        if (g != null)
                        {
                            this.Information = "Saving copied data to Target Store...";
                            this.Target.StorageProvider.SaveGraph(g);
                        }

                        //And finally since we've done a copy (not a move) so far we need to delete the original graph
                        //to effect a rename
                        if (this.Source.StorageProvider.DeleteSupported)
                        {
                            this.Information = "Removing source graph to complete the move operation";
                            this.Source.StorageProvider.DeleteGraph(this._sourceUri);

                            this.Information = "Move completed OK, Graph moved to '" + this._targetUri.ToSafeString() + "'" + (ReferenceEquals(this.Source, this.Target) ? String.Empty : " on " + this.Target);
                        }
                        else
                        {
                            this.Information = "Copy completed OK, Graph copied to '" + this._targetUri.ToSafeString() + "'" + (ReferenceEquals(this.Source, this.Target) ? String.Empty : " on " + this.Target) + ".  Please note that as the Source Triple Store does not support deleting Graphs so the Graph remains present in the Source Store";
                        }
                    }

                    break;

                case "Copy":
                    if (ReferenceEquals(this.Source, this.Target) && this.Source.StorageProvider is IUpdateableStorage)
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
                        ((IUpdateableStorage)this.Source.StorageProvider).Update(update.ToString());
                        this.Information = "COPY command completed OK, Graph copied to '" + this._targetUri.ToSafeString() + "'";
                    }
                    else
                    {
                        //Otherwise do a load of the source graph writing through to the target graph
                        IRdfHandler handler;
                        IGraph g = null;
                        if (this.Target.StorageProvider.UpdateSupported)
                        {
                            //If Target supports update then we'll use a WriteToStoreHandler combined with a GraphUriRewriteHandler
                            handler = new WriteToStoreHandler(this.Target.StorageProvider, this._targetUri);
                            handler = new GraphUriRewriteHandler(handler, this._targetUri);
                        }
                        else
                        {
                            //Otherwise we'll use a GraphHandler and do a save at the end
                            g = new Graph();
                            handler = new GraphHandler(g);
                        }
                        handler = new CopyMoveProgressHandler(handler, this, "Copying", this.Target.StorageProvider.UpdateSupported);
                        this._canceller = new CancellableHandler(handler);
                        if (this.HasBeenCancelled) this._canceller.Cancel();

                        //Now start reading out the data
                        this.Information = "Copying data from Graph '" + this._sourceUri.ToSafeString() + "' to '" + this._targetUri.ToSafeString() + "'";
                        this.Source.StorageProvider.LoadGraph(this._canceller, this._sourceUri);

                        //If we weren't moving the data directly need to save the resulting graph now
                        if (g != null)
                        {
                            this.Information = "Saving copied data to Store...";
                            this.Target.StorageProvider.SaveGraph(g);
                        }

                        this.Information = "Copy completed OK, Graph copied to '" + this._targetUri.ToSafeString() + "'" + (ReferenceEquals(this.Source, this.Target) ? String.Empty : " on " + this.Target.ToString());
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
        private readonly IRdfHandler _handler;
        private readonly CopyMoveTask _task;
        private readonly String _action;
        private readonly bool _streaming;
        private int _count;

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
