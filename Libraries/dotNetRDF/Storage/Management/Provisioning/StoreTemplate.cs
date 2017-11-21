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
using System.ComponentModel;
using System.Linq;

namespace VDS.RDF.Storage.Management.Provisioning
{
    /// <summary>
    /// A basic store template where the only parameter is the Store ID
    /// </summary>
    public class StoreTemplate
        : IStoreTemplate
    {
        /// <summary>
        /// Creates a new template
        /// </summary>
        /// <param name="id">Store ID</param>
        public StoreTemplate(String id)
            : this(id, "Unknown", String.Empty) { }

        /// <summary>
        /// Creates a new template
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <param name="name">Template Name</param>
        /// <param name="description">Template Description</param>
        public StoreTemplate(String id, String name, String description)
        {
            ID = id;
            TemplateName = name;
            TemplateDescription = description;
        }

        /// <summary>
        /// Gets/Sets the Store ID
        /// </summary>
        [Category("Basic"), Description("The ID of the Store")]
        public String ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the type of store the template will create
        /// </summary>
        [Category("Basic"), Description("Name of the type of store the template will create"), ReadOnly(true)]
        public String TemplateName
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the description of the type of store the template will create
        /// </summary>
        [Category("Basic"), Description("Description of the type of store the template will create"), ReadOnly(true)]
        public String TemplateDescription
        {
            get;
            protected set;
        }

        /// <summary>
        /// Validates the template
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This default implementation does no validation, derived classes must override this to add their required validation
        /// </remarks>
        public virtual IEnumerable<String> Validate()
        {
            return Enumerable.Empty<String>();
        }

        /// <summary>
        /// Gets the string representation of the template which is the Template Name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return TemplateName;
        }
    }
}
