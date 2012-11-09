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
    /// Definition for connections to Joseki
    /// </summary>
    [Obsolete("The Apache Jena project strongly recommends using Fuseki instead which is it's sucessor, Joseki is no longer supported by Apache Jena", true)]
    public class JosekiConnectionDefinition
        : BaseHttpConnectionDefinition
    {
        /// <summary>
        /// Creates a new Definition
        /// </summary>
        public JosekiConnectionDefinition()
            : base("Joseki", "Connect to a Joseki Server which exposes SPARQL based access to any Jena based stores e.g. SDB and TDB.", typeof(JosekiConnector)) { }

        /// <summary>
        /// Gets/Sets the Server URI
        /// </summary>
        [Connection(DisplayName = "Server URI", IsRequired = true, Type = ConnectionSettingType.String, DisplayOrder = -1, PopulateFrom = ConfigurationLoader.PropertyServer),
DefaultValue("http://localhost:2020")]
        public String Server
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the path to the Query endpoint
        /// </summary>
        [Connection(DisplayName = "Query Path", IsRequired = true, AllowEmptyString = false, PopulateFrom = ConfigurationLoader.PropertyQueryPath),
         DefaultValue("sparql")]
        public String QueryPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the path to the Update endpoint
        /// </summary>
        [Connection(DisplayName = "Update Path", IsRequired = false, AllowEmptyString = true, DisplaySuffix = "(Leave blank for read-only connection)", PopulateFrom = ConfigurationLoader.PropertyUpdatePath),
         DefaultValue("update")]
        public String UpdatePath
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
                return new JosekiConnector(this.Server, this.QueryPath, (String.IsNullOrEmpty(this.UpdatePath) ? null : this.UpdatePath), this.GetProxy());
            }
            else
            {
                return new JosekiConnector(this.Server, this.QueryPath, (String.IsNullOrEmpty(this.UpdatePath) ? null : this.UpdatePath));
            }
        }
    }
}
