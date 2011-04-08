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

namespace VDS.RDF
{
    /// <summary>
    /// A Graph which represents a Graph of a given Uri retrieved from some arbitrary Store
    /// </summary>
    /// <remarks>
    /// <para>
    /// Any Changes to this Graph locally are persisted to the underlying Storage using a Background Thread. This live persistence is only provided for <see cref="IGenericIOManager">IGenericIOManager</see> implementations which return <em>true</em> as the value of their <see cref="IGenericIOManager.UpdateSupported">UpdateSupported</see> property, otherwise persistence only occurs when the Graph is disposed of by using the <see cref="IGenericIOManager.SaveGraph">SaveGraph</see> method of the manager.
    /// </para>
    /// <para>
    /// If a process using this class terminates unexpectedly some changes may be lost.
    /// </para>
    /// <para>
    /// This class is designed to be <strong>completely</strong> agnostic of it's underlying Storage.  The Storage may be SQL Based for example a <see cref="MicrosoftSqlStoreManager">MicrosoftSqlStoreManager</see> supports <see cref="IGenericIOManager">IGenericIOManager</see> and can be used with this class though there are specific classes like <see cref="SqlGraph">SqlGraph</see> for SQL backed stores and a separate interface <see cref="ISqlIOManager">ISqlIOManager</see> dedicated to them.  The Storage may equally be some kind of Native RDF Storage such as a <see cref="VirtuosoManager">VirtuosoManager</see> which stores data in a Native Quad Store or a <see cref="TalisPlatformConnector">TalisPlatformConnector</see> which connects to the Talis paltform to access their SaaS solution for RDF Storage.
    /// </para>
    /// </remarks>
    [Obsolete("The StoreGraph has been superceded by the StoreGraphPersistenceWrapper", true)]
    public class StoreGraph : BackgroundPersistedGraph
    {
        /// <summary>
        /// Manager for Input/Output to and from the backing Store
        /// </summary>
        protected IGenericIOManager _manager;

        /// <summary>
        /// Empty Constructor for use by derived classes which don't wish to have the Graph automatically loaded
        /// </summary>
        protected StoreGraph() { }

        /// <summary>
        /// Finalizer for Store Graph which ensures <see cref="StoreGraph.Dispose">Dispose()</see> gets called if the user fails to explicitly call it
        /// </summary>
        ~StoreGraph()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Creates a new instance of a Store Graph which will contain the contents of the Graph with the given Uri
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to retrieve</param>
        /// <param name="manager">Generic Store Manager</param>
        public StoreGraph(Uri graphUri, IGenericIOManager manager) 
            : base()
        {
            this.BaseUri = graphUri;
            this._manager = manager;
            this._manager.LoadGraph(this, graphUri);
            this.Intialise();
        }

        /// <summary>
        /// Creates a new instance of a Store Graph which will contain the contents of the Graph with the given Uri
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to retrieve</param>
        /// <param name="manager">Generic Store Manager</param>
        public StoreGraph(String graphUri, IGenericIOManager manager) 
            : this(new Uri(graphUri), manager) { }

        #region Data Persistence to Generic Storage

        /// <summary>
        /// Initialises the Thread which will manage persistence of data using the Generic Store Manager
        /// </summary>
        protected override void Intialise()
        {
            if (this._manager.UpdateSupported)
            {
                //Only do persistence if the underlying Store supports Updates
                base.Intialise();
            }
        }

        /// <summary>
        /// Forces any changes to the Store Graph to be persisted to the underlying Store immediately
        /// </summary>
        public override void Flush()
        {
            //Can't Update if the underlying Store doesn't support Updates
            if (!this._manager.UpdateSupported) return;

            try
            {
                base.Flush();
            }
            catch (NotSupportedException noSupEx)
            {
                //We get this if the underlying Store doesn't support Updates but has erroneously returned true from its UpdateSupported property
                //Wrap in an RDF Storage Exception
                throw new RdfStorageException("The underlying Store for this Graph does not support Update at the Triple level.  Changes to this Graph cannot be automatically persisted to the underlying Store.", noSupEx);
            }
        }

        /// <summary>
        /// Updates the store by calling the underlying <see cref="IGenericIOManager">IGenericIOManager</see> instances <see cref="IGenericIOManager.UpdateGraph">UpdateGraph()</see> method
        /// </summary>
        protected override void UpdateStore()
        {
            this._manager.UpdateGraph(this._baseuri, this._addedTriplesBuffer, this._removedTriplesBuffer);
        }

        #endregion

        /// <summary>
        /// Disposes of a Store Graph
        /// </summary>
        public override void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Disposes of a Store Graph
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);

            //If we don't support Triple level updates update by persisting the entire Graph on Dispose
            if (!this._manager.UpdateSupported)
            {
                this._manager.SaveGraph(this);
            }

            base.Dispose();
            this._manager.Dispose();
        }
    }

    /// <summary>
    /// Class for representing an RDF Graph which is automatically stored to some arbitrary Store as it is modified
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed for situations where you wish to write data to a Graph in the Store where the data being added is not affected by existing data in the Graph.
    /// </para>
    /// <para>
    /// Unlike it's parent class <see cref="StoreGraph">StoreGraph</see> this class will not automatically load the contents of the existing Graph when instantiated.
    /// </para>
    /// <para>
    /// Iterating over the Triples/Nodes of this Graph will only return those that have been added to the Graph since its instantiation.
    /// </para>
    /// </remarks>
    [Obsolete("The WriteOnlyStoreGraph has been superceded by the StoreGraphPersistenceWrapper", true)]
    public class WriteOnlyStoreGraph : StoreGraph
    {
        /// <summary>
        /// Creates a new instance which will represent a write-only view of the Graph with the given URI
        /// </summary>
        /// <param name="graphUri">URI of the Graph to write to</param>
        /// <param name="manager">Generic Store Manager</param>
        public WriteOnlyStoreGraph(Uri graphUri, IGenericIOManager manager) 
            : base()
        {
            this.BaseUri = graphUri;
            this._manager = manager;
            this.Intialise();
        }

        /// <summary>
        /// Creates a new instance which will represent a write-only view of the Graph with the given URI
        /// </summary>
        /// <param name="graphUri">URI of the Graph to write to</param>
        /// <param name="manager">Generic Store Manager</param>
        public WriteOnlyStoreGraph(String graphUri, IGenericIOManager manager) 
            : this(new Uri(graphUri), manager) { }
    }
}

#endif