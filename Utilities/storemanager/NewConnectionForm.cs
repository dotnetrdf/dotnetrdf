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

                FlowLayoutPanel panel = new FlowLayoutPanel();
                panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                foreach (KeyValuePair<PropertyInfo, ConnectionAttribute> setting in def)
                {
                    Label label = new Label();
                    label.Text = setting.Value.DisplayName;
                    panel.Controls.Add(label);

                    switch (setting.Value.Type)
                    {
                        case ConnectionSettingType.String:
                        case ConnectionSettingType.Password:
                            TextBox box = new TextBox();
                            String s = (String)setting.Key.GetValue(def, null);
                            box.Text = (s != null) ? s : String.Empty;
                            panel.Controls.Add(box);
                            break;
                    }
                }

                this.grpConnectionSettings.Controls.Clear();
                this.grpConnectionSettings.Controls.Add(panel);
            }
        }
    }
}
