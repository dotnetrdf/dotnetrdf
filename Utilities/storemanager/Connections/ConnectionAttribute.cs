using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    
    public enum ConnectionSettingType
    {
        String = 0,
        Password = 1,
        Integer = 2,
        Boolean = 3,
        Enum = 4,
        File = 5
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
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

        public int DisplayOrder
        {
            get;
            set;
        }

        [DefaultValue(false)]
        public bool IsRequired
        {
            get;
            set;
        }

        [DefaultValue(false)]
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

        public String NotRequiredIf
        {
            get;
            set;
        }

        [DefaultValue(false)]
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

        public String FileFilter
        {
            get;
            set;
        }
    }
}
