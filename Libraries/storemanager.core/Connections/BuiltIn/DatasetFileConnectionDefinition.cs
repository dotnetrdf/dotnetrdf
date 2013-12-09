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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definitions for read-only connections to a Dataset File
    /// </summary>
    public class DatasetFileConnectionDefinition
        : BaseConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public DatasetFileConnectionDefinition()
            : base("Dataset File", "Allows you to access a Dataset File in NQuads, TriG or TriX format as a read-only store", typeof(DatasetFileManager)) { }

        /// <summary>
        /// Gets/Sets the File
        /// </summary>
        [Connection(DisplayName="Dataset File", DisplayOrder=1, IsRequired=true, AllowEmptyString=false, Type=ConnectionSettingType.File, FileFilter="RDF Dataset Files|*.nq;*.trig;*.trix;*.xml", PopulateFrom = ConfigurationLoader.PropertyFromFile)]
        public String File
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to load the file asynchronously
        /// </summary>
        [Connection(DisplayName = "Load the file asynchronously?", DisplayOrder = 2, Type = ConnectionSettingType.Boolean, PopulateFrom = ConfigurationLoader.PropertyAsync),
         DefaultValue(false)]
        public bool Async
        {
            get;
            set;
        }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            return new DatasetFileManager(this.File, this.Async);
        }

        /// <summary>
        /// Makes a copy of the current connection definition
        /// </summary>
        /// <returns>Copy of the connection definition</returns>
        public override IConnectionDefinition Copy()
        {
            DatasetFileConnectionDefinition definition = new DatasetFileConnectionDefinition();
            definition.File = this.File;
            definition.Async = this.Async;
            return definition;
        }
    }
}
