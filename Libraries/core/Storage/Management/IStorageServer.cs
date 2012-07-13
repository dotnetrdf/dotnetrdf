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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage.Management.Provisioning;

namespace VDS.RDF.Storage.Management
{
    /// <summary>
    /// Interface for storage servers which are systems capable of managing multiple stores which are exposed as <see cref="IStorageProvider"/> instances
    /// </summary>
    /// <remarks>
    /// This interface may be implemented either separately or alongside <see cref="IStorageProvider"/>.  It is quite acceptable for an implementation of <see cref="IStorageProvider"/> that provides a connection to a store sitting on a server that manages multiple stores to also provide an implementation of this interface in order to allow access to other stores on the server.
    /// </remarks>
    public interface IStorageServer<T>
        where T : IStoreTemplate
    {
        /// <summary>
        /// Gets the list of available stores
        /// </summary>
        /// <returns></returns>
        IEnumerable<String> ListStores();

        /// <summary>
        /// Gets a template for creating a store with the given ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        T GetNewTemplate(String id);

        /// <summary>
        /// Creates a new Store with the given ID
        /// </summary>
        /// <param name="template">Template for the new store</param>
        /// <returns>Whether creation succeeded</returns>
        /// <remarks>
        /// The template type provides the necessary information for the implementation to create a store, it is defined with a constrained generic type parameter so that implementations of this interface can further constrain the parameter to only accept implementations that make sense to them
        /// </remarks>
        bool CreateStore(T template);

        /// <summary>
        /// Deletes the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <remarks>
        /// Whether attempting to delete the Store that you are accessing is permissible is up to the implementation
        /// </remarks>
        void DeleteStore(string storeID);

        /// <summary>
        /// Gets the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        /// <remarks>
        /// If the implementation is also an instance of <see cref="IStorageProvider">IStorageProvider</see> and the requested Store ID represents the current instance then it is acceptable for an implementation to return itself.  Consumers of this method should be aware of this and if necessary use other means to create a connection to a store if they want a unique instance of the provider.
        /// </remarks>
        IStorageProvider GetStore(string storeID);
    }

    /// <summary>
    /// Interface for storage providers which are capable of managing multiple stores asynchronously
    /// </summary>
    public interface IAsyncStorageServer<T>
        where T : IStoreTemplate
    {
        /// <summary>
        /// Lists the available stores asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void ListStores(AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Gets a template for creating a store with the given ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        void GetNewTemplate(String id, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Creates a store asynchronously
        /// </summary>
        /// <param name="template">Template for the store to be created</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// Behaviour with regards to whether creating a store overwrites an existing store with the same ID is at the discretion of the implementation and <em>SHOULD</em> be documented in an implementations comments
        /// </remarks>
        void CreateStore(T template, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Deletes a store asynchronously
        /// </summary>
        /// <param name="storeID">ID of the store to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void DeleteStore(String storeID, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Gets a store asynchronously
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// If the implementation also implements <see cref="IAsyncStorageProvider"/> and the store ID requested matches the current instance an instance <em>MAY</em> invoke the callback immediately returning a reference to itself
        /// </remarks>
        void GetStore(String storeID, AsyncStorageCallback callback, Object state);
    }
}
