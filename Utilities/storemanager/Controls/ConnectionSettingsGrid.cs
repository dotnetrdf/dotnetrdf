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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Storage;
using VDS.RDF.Utilities.StoreManager.Connections;

namespace VDS.RDF.Utilities.StoreManager.Controls
{
    /// <summary>
    /// Control for editing connection settings based on a connection definition
    /// </summary>
    public partial class ConnectionSettingsGrid
        : UserControl
    {
        private IConnectionDefinition _def;
        private IStorageProvider _connection;

        /// <summary>
        /// Creates a new Connection Settings Grid
        /// </summary>
        public ConnectionSettingsGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a new Connection Settings Grid that displays the given definition
        /// </summary>
        /// <param name="def">Definition</param>
        public ConnectionSettingsGrid(IConnectionDefinition def)
            : this()
        {
            this.Render(def);
            this._def = def;
        }

        /// <summary>
        /// Renders the Connection Definition settings as user editable controls
        /// </summary>
        /// <param name="def">Definition</param>
        private void Render(IConnectionDefinition def)
        {
            this.lblDescrip.Text = def.StoreDescription;

            int i = 0;
            tblSettings.Controls.Clear();
            foreach (KeyValuePair<PropertyInfo, ConnectionAttribute> setting in def.OrderBy(s => s.Value.DisplayOrder))
            {
                if (setting.Value.Type != ConnectionSettingType.Boolean)
                {
                    Label label = new Label();
                    label.Text = setting.Value.DisplayName + ":";
                    label.TextAlign = ContentAlignment.MiddleLeft;
                    tblSettings.Controls.Add(label, 0, i);
                }

                switch (setting.Value.Type)
                {
                    case ConnectionSettingType.String:
                    case ConnectionSettingType.Password:
                        //String/Password so show a Textbox
                        TextBox box = new TextBox();
                        String s = (String)setting.Key.GetValue(def, null);
                        box.Text = (s != null) ? s : String.Empty;
                        box.Width = 225;
                        box.Tag = setting.Key;
                        if (setting.Value.Type == ConnectionSettingType.Password) box.PasswordChar = '*';

                        //Add the Event Handler which updates the Definition as the user types
                        box.TextChanged += new EventHandler((_sender, args) =>
                        {
                            (box.Tag as PropertyInfo).SetValue(def, box.Text, null);
                        });

                        //Show DisplaySuffix if relevant
                        if (!String.IsNullOrEmpty(setting.Value.DisplaySuffix))
                        {
                            FlowLayoutPanel flow = new FlowLayoutPanel();
                            flow.Margin = new Padding(0);
                            flow.Controls.Add(box);
                            Label suffix = new Label();
                            suffix.Text = setting.Value.DisplaySuffix;
                            suffix.AutoSize = true;
                            suffix.TextAlign = ContentAlignment.MiddleLeft;
                            flow.Controls.Add(suffix);
                            flow.WrapContents = false;
                            flow.AutoSize = true;
                            flow.AutoScroll = false;
                            tblSettings.Controls.Add(flow, 1, i);
                        }
                        else
                        {
                            tblSettings.Controls.Add(box, 1, i);
                        }
                        break;

                    case ConnectionSettingType.Boolean:
                        //Boolean so show a Checkbox
                        CheckBox check = new CheckBox();
                        check.AutoSize = true;
                        check.TextAlign = ContentAlignment.MiddleLeft;
                        check.CheckAlign = ContentAlignment.MiddleLeft;
                        check.Checked = (bool)setting.Key.GetValue(def, null);
                        check.Text = setting.Value.DisplayName;
                        check.Tag = setting.Key;

                        //Add the Event Handler which updates the Definition when the Checkbox changes
                        check.CheckedChanged += new EventHandler((_sender, args) =>
                        {
                            (check.Tag as PropertyInfo).SetValue(def, check.Checked, null);
                        });

                        this.tblSettings.SetColumnSpan(check, 2);
                        this.tblSettings.Controls.Add(check, 0, i);
                        break;

                    case ConnectionSettingType.Integer:
                        //Integer so show a Numeric Up/Down control
                        NumericUpDown num = new NumericUpDown();
                        num.ThousandsSeparator = true;
                        num.DecimalPlaces = 0;
                        int val = (int)setting.Key.GetValue(def, null);
                        if (setting.Value.IsValueRestricted)
                        {
                            num.Minimum = setting.Value.MinValue;
                            num.Maximum = setting.Value.MaxValue;
                        }
                        else
                        {
                            num.Minimum = Int32.MinValue;
                            num.Maximum = Int32.MaxValue;
                        }
                        num.Value = val;
                        num.Tag = setting.Key;

                        //Add the Event Handler which updates the Definition as the number changes
                        num.ValueChanged += new EventHandler((_sender, args) =>
                        {
                            (num.Tag as PropertyInfo).SetValue(def, (int)num.Value, null);
                        });

                        tblSettings.Controls.Add(num, 1, i);
                        break;

                    case ConnectionSettingType.Enum:
                        //Enum so show a ComboBox in DropDownList Mode
                        ComboBox ebox = new ComboBox();
                        ebox.DropDownStyle = ComboBoxStyle.DropDownList;
                        ebox.DataSource = Enum.GetValues(setting.Key.PropertyType);
                        ebox.SelectedItem = setting.Key.GetValue(def, null);
                        ebox.Tag = setting.Key;

                        //Add the Event Handler which updates the Definition as the selection changes
                        ebox.SelectedIndexChanged += new EventHandler((_sender, args) =>
                        {
                            (ebox.Tag as PropertyInfo).SetValue(def, (Enum)ebox.SelectedItem, null);
                        });

                        tblSettings.Controls.Add(ebox, 1, i);
                        break;

                    case ConnectionSettingType.File:
                        //File so show a TextBox and a Browse Button
                        String file = (String)setting.Key.GetValue(def, null);
                        FlowLayoutPanel fileFlow = new FlowLayoutPanel();
                        fileFlow.Margin = new Padding(0);
                        fileFlow.WrapContents = false;
                        fileFlow.AutoSize = true;
                        fileFlow.AutoScroll = false;

                        TextBox fileBox = new TextBox();
                        fileBox.Width = 225;
                        fileBox.Text = (file != null) ? file : String.Empty;
                        fileBox.Width = 225;
                        fileBox.Tag = setting.Key;
                        fileFlow.Controls.Add(fileBox);

                        Button browse = new Button();
                        browse.Text = "Browse";
                        browse.Tag = setting.Value;
                        fileFlow.Controls.Add(browse);

                        //Add the Event Handler which updates the Definition as the user types
                        fileBox.TextChanged += new EventHandler((_sender, args) =>
                        {
                            (fileBox.Tag as PropertyInfo).SetValue(def, fileBox.Text, null);
                        });

                        //Add the Event Handler for the Browse Button
                        browse.Click += new EventHandler((_sender, args) =>
                        {
                            ConnectionAttribute attr = browse.Tag as ConnectionAttribute;
                            if (attr == null) return;
                            this.ofdBrowse.Title = "Browse for " + attr.DisplayName;
                            this.ofdBrowse.Filter = (String.IsNullOrEmpty(attr.FileFilter) ? "All Files|*.*" : attr.FileFilter);
                            if (this.ofdBrowse.ShowDialog() == DialogResult.OK)
                            {
                                fileBox.Text = this.ofdBrowse.FileName;
                            }
                        });

                        tblSettings.Controls.Add(fileFlow, 1, i);

                        break;
                }

                i++;
            }
        }

        /// <summary>
        /// Gets/Sets the Connection Definition to display
        /// </summary>
        public IConnectionDefinition Definition
        {
            get
            {
                return this._def;
            }
            set
            {
                if (!ReferenceEquals(this._def, value))
                {
                    this._def = value;
                    this.Render(value);
                }
            }
        }

        /// <summary>
        /// Event Handler for Connect button click event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arugs</param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                this._connection = this._def.OpenConnection();
                if (this.chkForceReadOnly.Checked)
                {
                    if (this._connection is IQueryableStorage)
                    {
                        this._connection = new QueryableReadOnlyConnector((IQueryableStorage)this._connection);
                    }
                    else
                    {
                        this._connection = new ReadOnlyConnector(this._connection);
                    }
                }

                this.RaiseConnected();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection to " + this._def.StoreName + " Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Event that occurs when a connection is made
        /// </summary>
        public event Connected Connected;

        /// <summary>
        /// Helper for raising the connection event
        /// </summary>
        private void RaiseConnected()
        {
            Connected d = this.Connected;
            if (d != null)
            {
                d(this, new ConnectedEventArgs(this._connection));
            }
        }
    }


}
