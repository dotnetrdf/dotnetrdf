/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

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
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    public abstract class BaseConnectionDefinition
        : IConnectionDefinition
    {
        private Dictionary<PropertyInfo, ConnectionAttribute> _properties = new Dictionary<PropertyInfo, ConnectionAttribute>();

        public BaseConnectionDefinition(String storeName, String storeDescrip)
        {
            this.StoreName = storeName;
            this.StoreDescription = storeDescrip;

            //Discover decorated properties
            Type t = this.GetType();
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

        public string StoreName
        {
            get;
            private set;
        }

        public string StoreDescription
        {
            get;
            private set;
        }

        public IGenericIOManager OpenConnection()
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

        protected abstract IGenericIOManager OpenConnectionInternal();

        public IEnumerator<KeyValuePair<PropertyInfo, ConnectionAttribute>> GetEnumerator()
        {
            return this._properties.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}
