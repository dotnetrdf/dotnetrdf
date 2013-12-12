/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using VDS.RDF.Utilities.StoreManager.Connections;

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
            this.Source = form.Connection;
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
        /// Drag source connection
        /// </summary>
        public Connection Source
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
