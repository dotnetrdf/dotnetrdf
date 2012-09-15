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
using System.Text;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Possible Types of settings, used to determine how the UI for an annotated property should be displayed
    /// </summary>
    public enum ConnectionSettingType
    {
        /// <summary>
        /// String - displayed with a Text Box
        /// </summary>
        String = 0,
        /// <summary>
        /// Password - displayed with a Password Text Box
        /// </summary>
        Password = 1,
        /// <summary>
        /// Integer - displayed with a numeric selector
        /// </summary>
        Integer = 2,
        /// <summary>
        /// Boolean - displayed using a check box
        /// </summary>
        Boolean = 3,
        /// <summary>
        /// Enum - displayed using a combo box
        /// </summary>
        Enum = 4,
        /// <summary>
        /// File - displayed with a text box and a browse button
        /// </summary>
        File = 5
    }

    /// <summary>
    /// An attribute which can be applied to properties of a <see cref="IConnectionDefinition"/> implementation which help to define configuration information a user must enter to configure a connection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class ConnectionAttribute
        : Attribute
    {
        /// <summary>
        /// Display Name for the property
        /// </summary>
        public String DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Display Suffix to show after whatever UI element is rendered for the property
        /// </summary>
        public String DisplaySuffix
        {
            get;
            set;
        }

        /// <summary>
        /// Display Order
        /// </summary>
        public int DisplayOrder
        {
            get;
            set;
        }

        /// <summary>
        /// The URI for the Configuration Property that is used to serialize this piece of configuration for a connection created from this definition
        /// </summary>
        public String PopulateFrom
        {
            get;
            set;
        }

        /// <summary>
        /// The URI for a property defining an intermediate object via which the actual properties value may be populated using the property defined via the <see cref="ConnectionAttribute.PopulateFrom"/> property
        /// </summary>
        public String PopulateVia
        {
            get;
            set;
        }

        /// <summary>
        /// Is a value required?
        /// </summary>
        [DefaultValue(false)]
        public bool IsRequired
        {
            get;
            set;
        }

        /// <summary>
        /// Can the string value be empty?
        /// </summary>
        [DefaultValue(false)]
        public bool AllowEmptyString
        {
            get;
            set;
        }

        /// <summary>
        /// Type of the setting
        /// </summary>
        public ConnectionSettingType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Is the setting optional only if some other property is set?
        /// </summary>
        public String NotRequiredIf
        {
            get;
            set;
        }

        /// <summary>
        /// Is the value restricted?
        /// </summary>
        /// <remarks>
        /// Must be set to true if certain other value restriction attributes are specified or those will not be honoured
        /// </remarks>
        [DefaultValue(false)]
        public bool IsValueRestricted
        {
            get;
            set;
        }

        /// <summary>
        /// Minimum value of the setting
        /// </summary>
        /// <remarks>
        /// <para>
        /// For <see cref="ConnectionSettingType.Integer"/> settings determines the permitted value range, for <see cref="ConnectionSettingType.String"/>, <see cref="ConnectionSettingType.Password"/> and <see cref="ConnectionSettingType.File"/> attributes specified the permissible length of the value
        /// </para>
        /// <para>>
        /// You must also set <see cref="ConnectionAttribute.IsValueRestricted"/> for this to be honoured
        /// </para>
        /// </remarks>
        public int MinValue
        {
            get;
            set;
        }

        /// <summary>
        /// Maximum value of the setting
        /// </summary>
        /// <remarks>
        /// <para>
        /// For <see cref="ConnectionSettingType.Integer"/> settings determines the permitted value range, for <see cref="ConnectionSettingType.String"/>, <see cref="ConnectionSettingType.Password"/> and <see cref="ConnectionSettingType.File"/> attributes specified the permissible length of the value
        /// </para>
        /// <para>>
        /// You must also set <see cref="ConnectionAttribute.IsValueRestricted"/> for this to be honoured
        /// </para>
        /// </remarks>
        public int MaxValue
        {
            get;
            set;
        }

        /// <summary>
        /// Increment for the setting
        /// </summary>
        /// <remarks>
        /// Only applies to <see cref="ConnectionSettingType.Integer"/> settings, must also set <see cref="ConnectionAttribute.IsValueRestricted"/> for this to be honoured
        /// </remarks>
        public int Increment
        {
            get;
            set;
        }

        /// <summary>
        /// Filename filter for <see cref="ConnectionSettingType.File"/> settings
        /// </summary>
        public String FileFilter
        {
            get;
            set;
        }
    }
}
