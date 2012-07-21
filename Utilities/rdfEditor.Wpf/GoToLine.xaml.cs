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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Interaction logic for GoToLine.xaml
    /// </summary>
    public partial class GoToLine : Window
    {
        private int _line, _maxLine;

        public GoToLine(ITextEditorAdaptor<TextEditor> editor)
        {
            InitializeComponent();

            this._line = editor.Control.Document.GetLineByOffset(editor.CaretOffset).LineNumber;
            this._maxLine = editor.Control.Document.LineCount;
            this.txtLineNumber.Text = this._line.ToString();
            this.lblLineNumber.Content = String.Format((String)this.lblLineNumber.Content, this._maxLine);

            this.txtLineNumber.SelectAll();
            this.txtLineNumber.Focus();
        }

        public int Line
        {
            get
            {
                return this._line;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (Int32.TryParse(this.txtLineNumber.Text, out this._line))
            {
                if (this._line > 0 && this._line <= this._maxLine)
                {
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Line Number is not in the range 1-" + this._maxLine, "Invalid Line Number", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Not a valid Line Number!", "Invalid Line Number", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
