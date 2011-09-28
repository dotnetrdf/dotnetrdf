using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Utilities.StoreManager.Connections;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class NewConnectionForm : Form
    {
        private List<IConnectionDefinition> _definitions = new List<IConnectionDefinition>();

        public NewConnectionForm()
        {
            InitializeComponent();

            this._definitions.AddRange(ConnectionDefinitionManager.GetDefinitions());
            this.lstStoreTypes.DataSource = this._definitions;
            this.lstStoreTypes.DisplayMember = "StoreName";
        }

        private void lstStoreTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            IConnectionDefinition def = this.lstStoreTypes.SelectedItem as IConnectionDefinition;
            if (def != null)
            {
                this.lblDescrip.Text = def.StoreDescription;

                int i = 0;
                tblSettings.Controls.Clear();
                foreach (KeyValuePair<PropertyInfo, ConnectionAttribute> setting in def)
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
                            TextBox box = new TextBox();
                            String s = (String)setting.Key.GetValue(def, null);
                            box.Text = (s != null) ? s : String.Empty;
                            box.Width = 200;
                            if (setting.Value.Type == ConnectionSettingType.Password) box.PasswordChar = '*';
                            //TODO: Add support for DisplaySuffix
                            tblSettings.Controls.Add(box, 1, i);
                            break;

                        case ConnectionSettingType.Boolean:
                            CheckBox check = new CheckBox();
                            check.AutoSize = true;
                            check.TextAlign = ContentAlignment.MiddleLeft;
                            check.CheckAlign = ContentAlignment.MiddleLeft;
                            check.Checked = (bool)setting.Key.GetValue(def, null);
                            check.Text = setting.Value.DisplayName;
                            this.tblSettings.SetColumnSpan(check, 2);
                            this.tblSettings.Controls.Add(check, 0, i);
                            break;

                        case ConnectionSettingType.Integer:
                            NumericUpDown num = new NumericUpDown();
                            int val = (int)setting.Key.GetValue(def, null);
                            if (setting.Value.IsValueRestricted)
                            {
                                num.Value = val;
                                num.Minimum = setting.Value.MinValue;
                                num.Maximum = setting.Value.MaxValue;
                            }
                            else
                            {
                                num.Minimum = Int32.MinValue;
                                num.Maximum = Int32.MaxValue;
                                num.Value = val;
                            }
                            tblSettings.Controls.Add(num, 1, i);
                            break;
                    }

                    i++;
                }
            }
        }
    }
}
