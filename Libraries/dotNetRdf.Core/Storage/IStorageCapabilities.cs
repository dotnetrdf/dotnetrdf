/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Storage;

/// <summary>
/// Interface which describes the capabilities of some storage provider.
/// </summary>
public interface IStorageCapabilities
{
    /// <summary>
    /// Gets whether the connection with the underlying Store is ready for use.
    /// </summary>
    bool IsReady
    {
        get;
    }

    /// <summary>
    /// Gets whether the connection with the underlying Store is read-only.
    /// </summary>
    /// <remarks>
    /// Any Manager which indicates it is read-only should also return false for the <see cref="IStorageCapabilities.UpdateSupported">UpdatedSupported</see> property and should throw a <see cref="RdfStorageException">RdfStorageException</see> if the <strong>SaveGraph()</strong> or <strong>UpdateGraph()</strong> methods are called.
    /// </remarks>
    bool IsReadOnly
    {
        get;
    }

    /// <summary>
    /// Gets the Save Behaviour the Store uses.
    /// </summary>
    IOBehaviour IOBehaviour
    {
        get;
    }


    /// <summary>
    /// Gets whether the triple level updates are supported.
    /// </summary>
    /// <remarks>
    /// Some Stores do not support updates at the Triple level and may as designated in the interface defintion throw a <see cref="NotSupportedException">NotSupportedException</see> if the <strong>UpdateGraph()</strong> method is called.  This property allows for calling code to check in advance whether Updates are supported.
    /// </remarks>
    bool UpdateSupported
    {
        get;
    }

    /// <summary>
    /// Gets whether the deletion of graphs is supported.
    /// </summary>
    /// <remarks>
    /// Some Stores do not support the deletion of Graphs and may as designated in the interface definition throw a <see cref="NotSupportedException">NotSupportedException</see> if the <strong>DeleteGraph()</strong> method is called.  This property allows for calling code to check in advance whether Deletion of Graphs is supported.
    /// </remarks>
    bool DeleteSupported
    {
        get;
    }

    /// <summary>
    /// Gets whether the Store supports Listing Graphs.
    /// </summary>
    bool ListGraphsSupported
    {
        get;
    }
}