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
    /// Definition for connections to Virtuoso
    /// </summary>
    public class VirtuosoConnectionDefinition
        : BaseCredentialsRequiredServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new Definition
        /// </summary>
        public VirtuosoConnectionDefinition()
            : base("Virtuoso", "Connect to a Virtuoso Universal Server Quad Store", typeof(VirtuosoManager)) { }

        /// <summary>
        /// Gets/Sets the Server Port
        /// </summary>
        [Connection(DisplayName = "Port", DisplayOrder = 1, MinValue = 1, MaxValue = 65535, IsValueRestricted = true, Type = ConnectionSettingType.Integer, IsRequired = true, PopulateFrom = ConfigurationLoader.PropertyPort),
         DefaultValue(VirtuosoManager.DefaultPort)]
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Database
        /// </summary>
        [Connection(DisplayName = "Database", DisplayOrder = 2, IsRequired = true, AllowEmptyString = false, PopulateFrom = ConfigurationLoader.PropertyDatabase),
         DefaultValue(VirtuosoManager.DefaultDB)]
        public String Database
        {
            get;
            set;
        }

        /// <summary>
        /// Opens the Connection
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            return new VirtuosoManager(this.Server, this.Port, this.Database, this.Username, this.Password);
        }

        /// <summary>
        /// Makes a copy of the current connection definition
        /// </summary>
        /// <returns>Copy of the connection definition</returns>
        public override IConnectionDefinition Copy()
        {
            VirtuosoConnectionDefinition definition = new VirtuosoConnectionDefinition();
            definition.Server = this.Server;
            definition.Port = this.Port;
            definition.Database = this.Database;
            definition.Username = this.Username;
            definition.Password = this.Password;
            return definition;
        }
    }
}
