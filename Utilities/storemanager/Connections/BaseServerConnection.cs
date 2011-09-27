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
                foreach (DefaultValueAttribute attr in property.GetCustomAttributes(target, true).OfType<DefaultValueAttribute>())
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

    public abstract class BaseServerConnectionDefinition
        : BaseConnectionDefinition
    {
        public BaseServerConnectionDefinition(String storeName, String storeDescrip)
            : base(storeName, storeDescrip) { }

        [Connection(DisplayName="Server", IsRequired=true, Type = ConnectionSettingType.String),
        DefaultValue("localhost")]
        public virtual String Server
        {
            get;
            set;
        }
    }
}
