/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Storage.Management.Provisioning;

namespace VDS.RDF.Storage.Management
{
    /// <summary>
    /// Interface for storage servers which are systems capable of managing multiple stores which are exposed as <see cref="IStorageProvider"/> instances.
    /// </summary>
    /// <remarks>
    /// This interface may be implemented either separately or alongside <see cref="IStorageProvider"/>.  It is quite acceptable for an implementation of <see cref="IStorageProvider"/> that provides a connection to a store sitting on a server that manages multiple stores to also provide an implementation of this interface in order to allow access to other stores on the server.
    /// </remarks>
    public interface IStorageServer
        : IDisposable
    {
        /// <summary>
        /// Returns information on the IO behaviour of a Server.
        /// </summary>
        IOBehaviour IOBehaviour
        {
            get;
        }

        /// <summary>
        /// Gets the list of available stores.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> ListStores();

        /// <summary>
        /// Gets a default template for creating a store with the given ID.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <returns></returns>
        IStoreTemplate GetDefaultTemplate(string id);

        /// <summary>
        /// Gets all possible templates for creating a store with the given ID.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <returns></returns>
        IEnumerable<IStoreTemplate> GetAvailableTemplates(string id);

        /// <summary>
        /// Creates a new Store with the given ID.
        /// </summary>
        /// <param name="template">Template for the new store.</param>
        /// <returns>Whether creation succeeded.</returns>
        bool CreateStore(IStoreTemplate template);

        /// <summary>
        /// Deletes the Store with the given ID.
        /// </summary>
        /// <param name="storeID">Store ID.</param>
        /// <remarks>
        /// Whether attempting to delete the Store that you are accessing is permissible is up to the implementation.
        /// </remarks>
        void DeleteStore(string storeID);

        /// <summary>
        /// Gets the Store with the given ID.
        /// </summary>
        /// <param name="storeId">Store ID.</param>
        /// <returns></returns>
        IStorageProvider GetStore(string storeId);
    }

    /// <summary>
    /// Interface for storage providers which are capable of managing multiple stores asynchronously.
    /// </summary>
    public interface IAsyncStorageServer
        : IDisposable
    {
        /// <summary>
        /// Gets information on the IO Behaviour of the Server.
        /// </summary>
        IOBehaviour IOBehaviour
        {
            get;
        }

        /// <summary>
        /// Lists the available stores asynchronously.
        /// </summary>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        [Obsolete("This method is obsolete and will be removed in a future release. Replaced by ListStoresAsync.")]
        void ListStores(AsyncStorageCallback callback, object state);

        /// <summary>
        /// Lists the available stores asynchronously.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> ListStoresAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a default template for creating a store with the given ID.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        /// <returns></returns>
        [Obsolete("This method is obsolete and will be removed in a future release. Replaced by GetDefaultTemplateAsync.")]
        void GetDefaultTemplate(string id, AsyncStorageCallback callback, object state);

        /// <summary>
        /// Gets a default template for creating a store with the given ID.
        /// </summary>
        /// <param name="id">The store ID.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IStoreTemplate> GetDefaultTemplateAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Gets all available templates for creating a store with the given ID.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        [Obsolete("This method is obsolete and will be removed in a future release. Replaced by GetAvailableTemplatesAsync.")]
        void GetAvailableTemplates(string id, AsyncStorageCallback callback, object state);

        /// <summary>
        /// Gets all available templates for creating a store with the given ID.
        /// </summary>
        /// <param name="id">The store ID.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<IStoreTemplate>> GetAvailableTemplatesAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a store asynchronously.
        /// </summary>
        /// <param name="template">Template for the store to be created.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        /// <remarks>
        /// Behaviour with regards to whether creating a store overwrites an existing store with the same ID is at the discretion of the implementation and <em>SHOULD</em> be documented in an implementations comments.
        /// </remarks>
        [Obsolete("This method is obsolete and will be removed in a future release. Replaced by CreateStoreAsync.")]
        void CreateStore(IStoreTemplate template, AsyncStorageCallback callback, object state);

        /// <summary>
        /// Creates a store asynchronously.
        /// </summary>
        /// <param name="template">Template for the store to be created.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> CreateStoreAsync(IStoreTemplate template, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a store asynchronously.
        /// </summary>
        /// <param name="storeID">ID of the store to delete.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        [Obsolete("This method is obsolete and will be removed in a future release. Replaced by DeleteStoreAsync.")]
        void DeleteStore(string storeID, AsyncStorageCallback callback, object state);

        /// <summary>
        /// Deletes a store asynchronously.
        /// </summary>
        /// <param name="storeId">ID of the store to delete.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteStoreAsync(string storeId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a store asynchronously.
        /// </summary>
        /// <param name="storeId">Store ID.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        [Obsolete("This method is obsolete and will be removed in a future release. Replaced by GetStoreAsync.")]
        void GetStore(string storeId, AsyncStorageCallback callback, object state);

        /// <summary>
        /// Gets a store asynchronously.
        /// </summary>
        /// <param name="storeId">Store ID.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IAsyncStorageProvider> GetStoreAsync(string storeId, CancellationToken cancellationToken);
    }
}
