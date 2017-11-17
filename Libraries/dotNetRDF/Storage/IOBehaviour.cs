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
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Flags Enumeration which is used to express the IO Behaviours of a specific Store
    /// </summary>
    [Flags]
    public enum IOBehaviour
    {
        /// <summary>
        /// Indicates whether the Store is Read Only i.e. Saving is not supported
        /// </summary>
        IsReadOnly = 1,

        /// <summary>
        /// Indicates that the Store is a Triple Store
        /// </summary>
        IsTripleStore = 2,
        /// <summary>
        /// Indicates that the Store is a Quad (Graph) Store
        /// </summary>
        IsQuadStore = 4,

        /// <summary>
        /// Indicates whether the Store has an explicit unnamed default graph
        /// </summary>
        HasDefaultGraph = 8,
        /// <summary>
        /// Indicates whether the Store has named graphs
        /// </summary>
        HasNamedGraphs = 16,

        /// <summary>
        /// Indicates that a Triple Store appends Triples when the SaveGraph() method is used
        /// </summary>
        AppendTriples = 32,
        /// <summary>
        /// Indicates that a Triple Store overwrites Triples when the SaveGraph() method is used
        /// </summary>
        OverwriteTriples = 64,

        /// <summary>
        /// Indicates that Graph data written to the Default Graph is always appended when the SaveGraph() method is used
        /// </summary>
        AppendToDefault = 128,
        /// <summary>
        /// Indicates that Graph data written to the Default Graph overwrites existing data when the SaveGraph() method is used
        /// </summary>
        OverwriteDefault = 256,

        /// <summary>
        /// Indicates that Graph data written to Named Graphs is always appended when the SaveGraph() method is used
        /// </summary>
        AppendToNamed = 512,
        /// <summary>
        /// Indicates that Graph data written to Named Graphs overwrites existing data when the SaveGraph() method is used
        /// </summary>
        OverwriteNamed = 1024,

        /// <summary>
        /// Indicates a Store that can do Triple Level additions on existing Graphs using the UpdateGraph() method
        /// </summary>
        CanUpdateAddTriples = 2048,
        /// <summary>
        /// Indicates a Store that can do Triple Level removals on existing Graphs using the UpdateGraph() method
        /// </summary>
        CanUpdateDeleteTriples = 4096,

        /// <summary>
        /// Indicates that a Store has a notion of explicit empty graphs
        /// </summary>
        /// <remarks>
        /// For some quad stores the existence of a graph may only be defined in terms of one/more quads being stored in that graph
        /// </remarks>
        ExplicitEmptyGraphs = 8192,

        /// <summary>
        /// Indicates that the Store is from a system which provides access to multiple stores (such an implementation will usually implement the <see cref="IStorageServer">IStorageServer</see> interface) - at a minimum this usually means the store will allow you to list other available stores.  More complex abilities like creating and deleting stores are indicated by other flags.
        /// </summary>
        HasMultipleStores = 16384,
        /// <summary>
        /// Indicates that the Store provides the means to create additional Stores
        /// </summary>
        CanCreateStores = 32768,
        /// <summary>
        /// Indicates that the Store provides the means to delete Stores
        /// </summary>
        CanDeleteStores = 65536,

        /// <summary>
        /// Indicates a Store that can do Triple Level additions and removals on existing Graphs using the UpdateGraph() method
        /// </summary>
        CanUpdateTriples = CanUpdateAddTriples | CanUpdateDeleteTriples,

        /// <summary>
        /// Default Behaviour for Read Only Triple Stores
        /// </summary>
        ReadOnlyTripleStore = IsReadOnly | IsTripleStore,
        /// <summary>
        /// Default Behaviour for Read Only Quad (Graph) Stores
        /// </summary>
        ReadOnlyGraphStore = IsReadOnly | IsQuadStore | HasDefaultGraph | HasNamedGraphs,

        /// <summary>
        /// Default Behaviour for Triple Stores
        /// </summary>
        /// <remarks>
        /// Default Behaviour is considered to be a Triple Store where data is appended
        /// </remarks>
        TripleStore = IsTripleStore | AppendTriples,
        /// <summary>
        /// Default Behaviour for Quad (Graph) Stores
        /// </summary>
        /// <remarks>
        /// Default Behaviour is considered to be Quad Store with Default and Named Graphs, data is appended to the default graph and overwrites named graphs
        /// </remarks>
        GraphStore = IsQuadStore | HasDefaultGraph | HasNamedGraphs | AppendToDefault | OverwriteNamed,

        /// <summary>
        /// Behaviour for fully fledged storage servers i.e. multiple stores are supported and can be created and deleted as desired
        /// </summary>
        StorageServer = HasMultipleStores | CanCreateStores | CanDeleteStores,
    }
}
