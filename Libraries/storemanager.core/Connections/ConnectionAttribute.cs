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
