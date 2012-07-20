/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.ComponentModel;
using System.Linq;
using System.Text;

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
            this.ID = id;
            this.TemplateName = name;
            this.TemplateDescription = description;
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
        /// Gets the string representation of the template which is the Template Name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.TemplateName;
        }
    }
}
