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

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Interface for classes which provide the Read/Write functionality to some arbitrary Store
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> This is now a synonym for the more descriptive <see cref="IStorageProvider">IStorageProvider</see> interface and will be removed in future releases.
    /// </para>
    /// Designed to allow for arbitrary Triple Stores to be plugged into the library as required by the end user
    /// </remarks>
    [Obsolete("This interface is now a synonym for the IStorageProvider interface and remains in the API for backwards compatibility with existing code.  Please update your code to use IStorageProvider instead", false)]
    public interface IGenericIOManager 
        : IStorageProvider
    {
    }

    /// <summary>
    /// Interface for classes which provide SPARQL Query functionality to some arbitrary Store in addition to Read/Write functionality
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> This is now a synonym for the more descriptive <see cref="IQueryableStorage">IQueryableStorage</see> interface and will be removed in future releases.
    /// </para>
    /// Designed to allow for arbitrary Triple Stores to be plugged into the library as required by the end user
    /// </remarks>
    [Obsolete("This interface is now a synonym for the IQueryableStorage interface and remains in the API for backwards compatibility with existing code.  Please update your code to use IQueryableStorage instead", false)]
    public interface IQueryableGenericIOManager 
        : IGenericIOManager, IQueryableStorage
    {

    }

    /// <summary>
    /// Interface for classes which provide SPARQL Update functionality to some arbitrary Store in addition to Read/Write functionality
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> This is now a synonym for the more descriptive <see cref="IUpdateableStorage">IUpdateableStorage</see> interface and will be removed in future releases.
    /// </para>
    /// Designed to allow for arbitrary Triple Stores to be plugged into the library as required by the end user
    /// </remarks>
    [Obsolete("This interface is now a synonym for the IUpdateableStorage interface and remains in the API for backwards compatibility with existing code.  Please update your code to use IUpdateableStorage instead", false)]
    public interface IUpdateableGenericIOManager
        : IQueryableGenericIOManager, IUpdateableStorage
    {

    }

    /// <summary>
    /// Interfaces for classes which provide the ability to create and delete new stores
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> This is now a synonym for the more descriptive <see cref="IStorageServer">IStorageServer</see> interface and will be removed in future releases.
    /// </para>
    /// </remarks>
    [Obsolete("This interface is now a synonym for the IStorageServer interface and remains in the API for backwards compatibility with existing code.  Please update your code to use IStorageServer instead", false)]
    public interface IMultiStoreGenericIOManager 
        : IStorageServer
    {

    }
}