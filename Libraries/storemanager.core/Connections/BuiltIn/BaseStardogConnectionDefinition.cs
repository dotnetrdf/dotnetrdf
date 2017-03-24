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
using System.ComponentModel;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Base connection definition for Stardog stores
    /// </summary>
    public abstract class BaseStardogConnectionDefinition
        : BaseHttpServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        /// <param name="storeName">Store name</param>
        /// <param name="storeDescrip">Store description</param>
        /// <param name="t">Type</param>
        protected BaseStardogConnectionDefinition(string storeName, string storeDescrip, Type t) 
            : base(storeName, storeDescrip, t) { }

        /// <summary>
        /// Gets/Sets the Server URI
        /// </summary>
        [Connection(DisplayName = "Server URI", IsRequired = true, AllowEmptyString = false, DisplayOrder = -1, PopulateFrom = ConfigurationLoader.PropertyServer),
         DefaultValue("http://localhost:5820/")]
        public override String Server
        {
            get { return base.Server; }
            set { base.Server = value; }
        }

        /// <summary>
        /// Gets/Sets the Store ID
        /// </summary>
        [Connection(DisplayName = "Store ID", DisplayOrder = 1, AllowEmptyString = false, IsRequired = true, PopulateFrom = ConfigurationLoader.PropertyStore)]
        public String StoreID { get; set; }

        /// <summary>
        /// Gets/Sets the Reasoning Mode for queries
        /// </summary>
        [Connection(DisplayName = "Query Reasoning Mode", DisplayOrder = 2, Type = ConnectionSettingType.Enum, PopulateFrom = ConfigurationLoader.PropertyLoadMode),
         DefaultValue(StardogReasoningMode.None)]
        public virtual StardogReasoningMode ReasoningMode { get; set; }

        /// <summary>
        /// Gets/Sets the Username
        /// </summary>
        [Connection(DisplayName = "Username", DisplayOrder = 3, IsRequired = false, AllowEmptyString = true, PopulateFrom = ConfigurationLoader.PropertyUser)]
        public String Username { get; set; }

        /// <summary>
        /// Gets/Sets the Password
        /// </summary>
        [Connection(DisplayName = "Password", DisplayOrder = 4, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.Password, PopulateFrom = ConfigurationLoader.PropertyPassword)]
        public String Password { get; set; }

        /// <summary>
        /// Gets/Sets whether to use the Stardog anonymous account
        /// </summary>
        [Connection(DisplayName = "Use the Stardog default anonymous user account instead of an explicit Username and Password?", DisplayOrder = 5, Type = ConnectionSettingType.Boolean),
         DefaultValue(false)]
        public bool UseAnonymousAccount { get; set; }

        public override string ToString()
        {
            String mode = String.Empty;
            switch (this.ReasoningMode)
            {
                case StardogReasoningMode.QL:
                    mode = " (OWL QL Reasoning)";
                    break;
                case StardogReasoningMode.EL:
                    mode = " (OWL EL Reasoning)";
                    break;
                case StardogReasoningMode.RL:
                    mode = " (OWL RL Reasoning)";
                    break;
                case StardogReasoningMode.DL:
                    mode = " (OWL DL Reasoning)";
                    break;
                case StardogReasoningMode.RDFS:
                    mode = " (RDFS Reasoning)";
                    break;
                case StardogReasoningMode.SL:
                    mode = " (SL Reasoning)";
                    break;
            }
            return "[Stardog] Knowledge Base '" + this.StoreID.ToSafeString() + "' on Server '" + this.Server.ToSafeString() + "'" + mode;
        }
    }
}