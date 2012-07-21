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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Interaction logic for RdfWriterOptionsWindow.xaml
    /// </summary>
    public partial class RdfWriterOptionsWindow : Window
    {
        private IRdfWriter _writer;

        public RdfWriterOptionsWindow(IRdfWriter writer)
        {
            InitializeComponent();

            //Show Compression Levels
            Type clevels = typeof(WriterCompressionLevel);
            foreach (FieldInfo field in clevels.GetFields())
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = field.Name;
                item.Tag = field.GetValue(null);
                this.cboCompressionLevel.Items.Add(item);
                if (field.Name.Equals("Default"))
                {
                    this.cboCompressionLevel.SelectedItem = item;
                }
            }
            if (this.cboCompressionLevel.SelectedItem == null && this.cboCompressionLevel.Items.Count > 0)
            {
                this.cboCompressionLevel.SelectedItem = this.cboCompressionLevel.Items[0];
            }

            //Enable/Disable relevant controls
            this.cboCompressionLevel.IsEnabled = (writer is ICompressingWriter);
            this.chkHighSpeed.IsEnabled = (writer is IHighSpeedWriter);
            this.chkPrettyPrint.IsEnabled = (writer is IPrettyPrintingWriter);
            this.chkUseAttributes.IsEnabled = (writer is IAttributeWriter);
            this.chkUseDtds.IsEnabled = (writer is IDtdWriter);
            this.stkHtmlWriter.IsEnabled = (writer is IHtmlWriter);
            this.stkXmlWriter.IsEnabled = (writer is IDtdWriter || writer is IAttributeWriter);

            this._writer = writer;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //Apply the selected Options
            if (this._writer is ICompressingWriter)
            {
                try
                {
                    int? cLevel = ((ComboBoxItem)this.cboCompressionLevel.SelectedItem).Tag as int?;
                    if (cLevel != null) ((ICompressingWriter)this._writer).CompressionLevel = cLevel.Value;
                }
                catch
                {
                    //Can't set Compression Level so skip
                }
            }
            if (this._writer is IHighSpeedWriter)
            {
                ((IHighSpeedWriter)this._writer).HighSpeedModePermitted = this.chkHighSpeed.IsChecked.Value;
            }
            if (this._writer is IPrettyPrintingWriter)
            {
                ((IPrettyPrintingWriter)this._writer).PrettyPrintMode = this.chkPrettyPrint.IsChecked.Value;
            }
            if (this._writer is IAttributeWriter)
            {
                ((IAttributeWriter)this._writer).UseAttributes = this.chkUseAttributes.IsChecked.Value;
            }
            if (this._writer is IDtdWriter)
            {
                ((IDtdWriter)this._writer).UseDtd = this.chkUseDtds.IsChecked.Value;
            }
            if (this._writer is IHtmlWriter)
            {
                IHtmlWriter htmlWriter = (IHtmlWriter)this._writer;
                htmlWriter.Stylesheet = this.txtStylesheet.Text;
                htmlWriter.CssClassBlankNode = this.ToUnsafeString(this.txtCssClassBNodes.Text);
                htmlWriter.CssClassDatatype = this.ToUnsafeString(this.txtCssClassDatatypes.Text);
                htmlWriter.CssClassLangSpec = this.ToUnsafeString(this.txtCssClassLangSpec.Text);
                htmlWriter.CssClassLiteral = this.ToUnsafeString(this.txtCssClassLiterals.Text);
                htmlWriter.CssClassUri = this.ToUnsafeString(this.txtCssClassUri.Text);
                htmlWriter.UriPrefix = this.txtPrefixUris.Text;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private String ToUnsafeString(String value)
        {
            return (value.Equals(String.Empty) ? null : value);
        }
    }
}
