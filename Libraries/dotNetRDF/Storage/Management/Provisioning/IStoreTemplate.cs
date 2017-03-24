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

namespace VDS.RDF.Storage.Management.Provisioning
{
    /// <summary>
    /// Interface for templates for the provisioning of new stores
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is intentionally very limited, the generic type constraints on the <see cref="IStorageServer"/> interface allow for specific implementations of that interface to futher constrain their implementation to accept only relevant implementations of this interface when provisioning new stores.
    /// </para>
    /// <para>
    /// Specific implementations will likely add various properties that allow end users to configure implementation specific parameters.  It is suggested that implementors include System.ComponentModel attributes on their implementations.
    /// </para>
    /// </remarks>
    public interface IStoreTemplate
    {
        /// <summary>
        /// Gets/Sets the ID for the Store
        /// </summary>
        String ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the type of store the template will create
        /// </summary>
        String TemplateName
        {
            get;
        }

        /// <summary>
        /// Gets the description of the type of store the template will create
        /// </summary>
        String TemplateDescription
        {
            get;
        }

        /// <summary>
        /// Validates the template returning an enumeration of error messages
        /// </summary>
        /// <returns></returns>
        IEnumerable<String> Validate();
    }
}
