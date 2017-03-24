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
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definition for connections to 4store
    /// </summary>
    public class FourStoreConnectionDefinition
        : BaseHttpServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public FourStoreConnectionDefinition()
            : base("4store", "Connect to a 4store server", typeof(FourStoreConnector)) { }

        /// <summary>
        /// Gets/sets whether to enable SPARQL Update support
        /// </summary>
        [Connection(DisplayName = "Enable SPARQL Update support? (Requires 4store 1.1.x)", DisplayOrder = 1, Type = ConnectionSettingType.Boolean, PopulateFrom = ConfigurationLoader.PropertyEnableUpdates),
         DefaultValue(true)]
        public bool EnableUpdates
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
            if (this.UseProxy)
            {
                return new FourStoreConnector(this.Server, this.EnableUpdates, this.GetProxy());   
            }
            return new FourStoreConnector(this.Server, this.EnableUpdates);
        }

        /// <summary>
        /// Makes a copy of the current connection definition
        /// </summary>
        /// <returns>Copy of the connection definition</returns>
        public override IConnectionDefinition Copy()
        {
            FourStoreConnectionDefinition definition = new FourStoreConnectionDefinition();
            definition.Server = this.Server;
            definition.EnableUpdates = this.EnableUpdates;
            definition.ProxyPassword = this.ProxyPassword;
            definition.ProxyUsername = this.ProxyUsername;
            definition.ProxyServer = this.ProxyServer;
            return definition;
        }

        public override string ToString()
        {
            return "[4store] " + this.Server.ToSafeString();
        }
    }
}
