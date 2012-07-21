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
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Information for doing copy/move via drag/drop
    /// </summary>
    class CopyMoveDragInfo
    {
        /// <summary>
        /// Creates a new Copy/Move infor
        /// </summary>
        /// <param name="form">Drag Source</param>
        /// <param name="sourceUri">Source Graph URI</param>
        public CopyMoveDragInfo(StoreManagerForm form, String sourceUri)
        {
            this.Form = form;
            this.Source = form.Manager;
            this.SourceUri = sourceUri;
        }

        /// <summary>
        /// Drag Source Form
        /// </summary>
        public StoreManagerForm Form
        {
            get;
            private set;
        }

        /// <summary>
        /// Drag Source Storage Provider
        /// </summary>
        public IStorageProvider Source
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Source Graph URI
        /// </summary>
        public String SourceUri
        {
            get;
            private set;
        }
    }
}
