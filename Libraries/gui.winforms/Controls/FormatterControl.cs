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
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.GUI.WinForms.Controls
{
    /// <summary>
    /// A control which provides the means to select a node formatter
    /// </summary>
    public partial class FormatterControl : UserControl
    {
        private Formatter _defaultFormatter;
        private readonly List<Formatter> _formatters = new List<Formatter>();

        /// <summary>
        /// Creates a new formatter control
        /// </summary>
        public FormatterControl()
        {
            InitializeComponent();

            //Load Formatters
            Type targetType = typeof (INodeFormatter);
            foreach (Type t in Assembly.GetAssembly(targetType).GetTypes())
            {
                if (t.Namespace == null) continue;

                if (!t.Namespace.Equals("VDS.RDF.Writing.Formatting")) continue;
                if (!t.GetInterfaces().Contains(targetType)) continue;
                try
                {
                    INodeFormatter formatter = (INodeFormatter) Activator.CreateInstance(t);
                    this._formatters.Add(new Formatter(formatter.GetType(), formatter.ToString()));
                }
                catch
                {
                    //Ignore this Formatter
                }
            }
            this._formatters.Sort();

            this.cboFormat.DataSource = this._formatters;
            this.cboFormat.SelectedItem = this._defaultFormatter ?? this._formatters.First();
            this.cboFormat.SelectedIndexChanged += cboFormat_SelectedIndexChanged;
            this.RaiseFormatterChanged();
        }

        private void cboFormat_SelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            if (cboFormat.SelectedItem == null) return;
            this.CurrentFormatter = (Formatter) cboFormat.SelectedItem;
            this.RaiseFormatterChanged();
        }

        /// <summary>
        /// Gets/Sets the default formatter
        /// </summary>
        public Type DefaultFormatter
        {
            get { return this._defaultFormatter != null ? this._defaultFormatter.Type : null; }
            set
            {
                if (value == null) return;
                Formatter formatter = this._formatters.FirstOrDefault(f => f.Type == value);
                if (formatter == null) return;
                this._defaultFormatter = formatter;
                this.cboFormat.SelectedItem = this._defaultFormatter;
                this.RaiseFormatterChanged();
            }
        }

        /// <summary>
        /// Gets/Sets the current formatter
        /// </summary>
        public Formatter CurrentFormatter { get; set; }

        /// <summary>
        /// Gets an instance of the currently selected formatter
        /// </summary>
        /// <returns></returns>
        public INodeFormatter GetFormatter()
        {
            return this.GetFormatter(null);
        }

        /// <summary>
        /// Gets an instance of the currently selected formatter using the given namespaces if possible
        /// </summary>
        /// <param name="namespaces">Namespaces</param>
        /// <returns></returns>
        public INodeFormatter GetFormatter(INamespaceMapper namespaces)
        {
            return this.CurrentFormatter.CreateInstance(namespaces);
        }

        /// <summary>
        /// Helper method for raising the FormatterChanged event
        /// </summary>
        protected void RaiseFormatterChanged()
        {
            FormatterChanged d = this.FormatterChanged;
            if (d == null) return;
            d(this, this.CurrentFormatter);
        }

        /// <summary>
        /// Event which is raised when the formatter is changed
        /// </summary>
        public event FormatterChanged FormatterChanged;
    }

    /// <summary>
    /// Represents a formatter
    /// </summary>
    public class Formatter
        : IComparable<Formatter>
    {
        /// <summary>
        /// Creates a new formatter
        /// </summary>
        /// <param name="t">Formatter Type</param>
        /// <param name="name">Friendly Name</param>
        public Formatter(Type t, String name)
        {
            this.Type = t;
            this.Name = name;
        }

        /// <summary>
        /// Gets the type
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets/Sets the name
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Creates an instance of the given formatter using the namespaces provided if possible
        /// </summary>
        /// <param name="namespaces">Namespaces</param>
        /// <returns></returns>
        public INodeFormatter CreateInstance(INamespaceMapper namespaces)
        {
            if (namespaces != null)
            {
                try
                {
                    INodeFormatter formatter = (INodeFormatter) Activator.CreateInstance(this.Type, new object[] {namespaces});
                    return formatter;
                }
                catch
                {
                    // Ignore
                }
            }
            try
            {
                INodeFormatter formatter = (INodeFormatter) Activator.CreateInstance(this.Type);
                return formatter;
            }
            catch (Exception)
            {
                // Fallback to default formatter
                return new SparqlFormatter();
            }
        }

        /// <summary>
        /// Gets the string representation of the formatter which is its friendly name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Compares this formatter to another
        /// </summary>
        /// <param name="other">Other formatter</param>
        /// <returns></returns>
        public int CompareTo(Formatter other)
        {
            return String.CompareOrdinal(this.Name, other.Name);
        }
    }
}