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
using System.Reflection;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Abstract Base Class for connection definitions, includes implementation for honouring the restrictions on settings that <see cref="ConnectionAttribute"/> annotations may define
    /// </summary>
    public abstract class BaseConnectionDefinition
        : IConnectionDefinition
    {
        private Dictionary<PropertyInfo, ConnectionAttribute> _properties = new Dictionary<PropertyInfo, ConnectionAttribute>();

        /// <summary>
        /// Creates a new Connection Definition
        /// </summary>
        /// <param name="storeName">Display Name of the Store</param>
        /// <param name="storeDescrip">Display Description of the Store</param>
        /// <param name="t">Type of the connection instance that this definition will generate</param>
        public BaseConnectionDefinition(String storeName, String storeDescrip, Type t)
        {
            this.StoreName = storeName;
            this.StoreDescription = storeDescrip;
            this.Type = t;

            //Discover decorated properties
            t = this.GetType();
            Type target = typeof(ConnectionAttribute);
            Type def = typeof(DefaultValueAttribute);
            foreach (PropertyInfo property in t.GetProperties())
            {
                foreach (ConnectionAttribute attr in property.GetCustomAttributes(target, true).OfType<ConnectionAttribute>())
                {
                    this._properties.Add(property, attr);
                    break;
                }
                foreach (DefaultValueAttribute attr in property.GetCustomAttributes(def, true).OfType<DefaultValueAttribute>())
                {
                    property.SetValue(this, attr.Value, null);
                }
            }
        }

        /// <summary>
        /// Gets the Display Name of the Store
        /// </summary>
        public string StoreName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Display Description of the Store
        /// </summary>
        public string StoreDescription
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the .Net Type that connections created from this definition will have
        /// </summary>
        public Type Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Opens a connection
        /// </summary>
        /// <remarks>
        /// First validates that the settings for the connection are valid per their <see cref="ConnectionAttribute"/> annotations and then invokes the abstract method <see cref="BaseConnectionDefinition.OpenConnectionInternal()"/> which implementers should use to actually return the new connection
        /// </remarks>
        /// <returns></returns>
        public IStorageProvider OpenConnection()
        {
            //Validate Attributes
            foreach (PropertyInfo property in this._properties.Keys)
            {
                ConnectionAttribute attr = this._properties[property];

                switch (attr.Type)
                {
                    case ConnectionSettingType.Boolean:
                        bool b = (bool)property.GetValue(this, null);
                        break;

                    case ConnectionSettingType.Integer:
                        int i = (int)property.GetValue(this, null);
                        if (attr.IsValueRestricted)
                        {
                            if (i < attr.MinValue)
                            {
                                throw new Exception(attr.DisplayName + " must be an Integer above " + attr.MinValue);
                            }
                            else if (i > attr.MaxValue)
                            {
                                throw new Exception(attr.DisplayName + " must be an Integer below " + attr.MaxValue);
                            }
                        }
                        break;

                    case ConnectionSettingType.Password:
                    case ConnectionSettingType.String:
                    case ConnectionSettingType.File:
                        String s = (String)property.GetValue(this, null);
                        if (attr.IsRequired)
                        {
                            if (s == null || (s.Equals(String.Empty) && !attr.AllowEmptyString))
                            {
                                if (attr.NotRequiredIf != null)
                                {
                                    PropertyInfo notReqProp = this._properties.Keys.Where(p => p.Name.Equals(attr.NotRequiredIf)).FirstOrDefault();
                                    if (notReqProp != null)
                                    {
                                        bool notReq = (bool)notReqProp.GetValue(this, null);
                                        if (!notReq)
                                        {
                                            throw new Exception(attr.DisplayName + " is a required connection setting!");
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Value for NotRequiredIf property is invalid");
                                    }
                                }
                                else
                                {
                                    throw new Exception(attr.DisplayName + " is a required connection setting!");
                                }
                            }
                        }
                        if (s != null && attr.IsValueRestricted)
                        {
                            if (s.Length < attr.MinValue)
                            {
                                throw new Exception(attr.DisplayName + " must be at least " + attr.MinValue + " characters long");
                            }
                            else if (s.Length > attr.MaxValue)
                            {
                                throw new Exception(attr.DisplayName + " must be no more than " + attr.MaxValue + " characters long");
                            }
                        }
                        break;

                    case ConnectionSettingType.Enum:
                        Enum e = (Enum)property.GetValue(this, null);
                        break;

                    default:
                        throw new Exception("Not a valid Connection Setting Type");
                }
            }
            return this.OpenConnectionInternal();
        }

        /// <summary>
        /// Opens a connection, called after setting validation has taken place
        /// </summary>
        /// <returns></returns>
        protected abstract IStorageProvider OpenConnectionInternal();

        /// <summary>
        /// Populates the settings from an existing serialized configuration
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="objNode">Object Node</param>
        public virtual void PopulateFrom(IGraph g, INode objNode)
        {
            foreach (PropertyInfo property in this._properties.Keys)
            {
                ConnectionAttribute attr = this._properties[property];

                if (!String.IsNullOrEmpty(attr.PopulateFrom))
                {
                    INode n = objNode;

                    if (!String.IsNullOrEmpty(attr.PopulateVia))
                    {
                        n = ConfigurationLoader.GetConfigurationNode(g, n, ConfigurationLoader.CreateConfigurationNode(g, attr.PopulateVia));
                        if (n == null) continue;
                    }

                    switch (attr.Type)
                    {
                        case ConnectionSettingType.Boolean:
                            bool b = ConfigurationLoader.GetConfigurationBoolean(g, n, ConfigurationLoader.CreateConfigurationNode(g, attr.PopulateFrom), (bool)property.GetValue(this, null));
                            property.SetValue(this, b, null);
                            break;

                        case ConnectionSettingType.File:
                        case ConnectionSettingType.Password:
                        case ConnectionSettingType.String:
                            String s = ConfigurationLoader.GetConfigurationString(g, n, ConfigurationLoader.CreateConfigurationNode(g, attr.PopulateFrom));
                            if (!String.IsNullOrEmpty(s))
                            {
                                property.SetValue(this, s, null);
                            }
                            else
                            {
                                //May be a URI as the object
                                IUriNode u = ConfigurationLoader.GetConfigurationNode(g, n, ConfigurationLoader.CreateConfigurationNode(g, attr.PopulateFrom)) as IUriNode;
                                if (u != null) property.SetValue(this, u.Uri.AbsoluteUri, null);
                            }
                            break;

                        case ConnectionSettingType.Integer:
                            int i = ConfigurationLoader.GetConfigurationInt32(g, n, ConfigurationLoader.CreateConfigurationNode(g, attr.PopulateFrom), (int)property.GetValue(this, null));
                            property.SetValue(this, i, null);
                            break;

                        case ConnectionSettingType.Enum:
                            String enumStr = ConfigurationLoader.GetConfigurationString(g, n, ConfigurationLoader.CreateConfigurationNode(g, attr.PopulateFrom));
                            if (!String.IsNullOrEmpty(enumStr))
                            {
                                try
                                {
                                    Object val = Enum.Parse(property.GetValue(this, null).GetType(), enumStr, false);
                                    property.SetValue(this, val, null);
                                }
                                catch
                                {
                                    //Ignore errors
                                }
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the available <see cref="ConnectionAttribute"/> annotated settings for the connection
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<PropertyInfo, ConnectionAttribute>> GetEnumerator()
        {
            return this._properties.GetEnumerator();
        }

        /// <summary>
        /// Gets the available <see cref="ConnectionAttribute"/> annotated settings for the connection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}
