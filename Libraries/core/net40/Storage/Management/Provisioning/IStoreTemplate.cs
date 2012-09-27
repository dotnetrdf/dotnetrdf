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
using System.Linq;
using System.Text;

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
