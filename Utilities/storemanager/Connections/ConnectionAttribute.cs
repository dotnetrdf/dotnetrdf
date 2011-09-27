using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    
    public enum ConnectionSettingType
    {
        String = 0,
        Password = 1,
        Integer = 2,
        Boolean = 3
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    public class ConnectionAttribute
        : Attribute
    {
        public String DisplayName
        {
            get;
            set;
        }

        public String DisplaySuffix
        {
            get;
            set;
        }

        public bool IsRequired
        {
            get;
            set;
        }

        public bool AllowEmptyString
        {
            get;
            set;
        }

        public ConnectionSettingType Type
        {
            get;
            set;
        }

        public String Default
        {
            get;
            set;
        }

        public String NotRequiredIf
        {
            get;
            set;
        }

        public bool IsValueRestricted
        {
            get;
            set;
        }

        public int MinValue
        {
            get;
            set;
        }

        public int MaxValue
        {
            get;
            set;
        }
    }
}
