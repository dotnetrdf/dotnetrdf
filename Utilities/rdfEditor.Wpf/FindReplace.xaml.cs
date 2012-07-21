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
using System.Text.RegularExpressions;
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
    /// Interaction logic for FindReplace.xaml
    /// </summary>
    public partial class FindReplace : Window
    {
        private FindReplaceMode _mode = FindReplaceMode.Find;
        private WpfFindAndReplace _engine = new WpfFindAndReplace();

        public FindReplace()
        {
            InitializeComponent();
            this.GotFocus += new RoutedEventHandler(FindReplace_GotFocus);
        }

        void FindReplace_GotFocus(object sender, RoutedEventArgs e)
        {
            Window_GotFocus(sender, e);
        }

        public FindReplaceMode Mode
        {
            get
            {
                return this._mode;
            }
            set
            {
                this._mode = value;
                if (this._mode == FindReplaceMode.Find)
                {
                    this.ToggleReplaceVisibility(Visibility.Collapsed);
                }
                else
                {
                    this.ToggleReplaceVisibility(Visibility.Visible);
                }
            }
        }

        public ITextEditorAdaptor<TextEditor> Editor
        {
            get;
            set;
        }

        public void FindNext()
        {
            this._engine.Find(this.Editor);
        }

        private void ToggleReplaceVisibility(Visibility v)
        {
            this.lblReplace.Visibility = v;
            this.cboReplace.Visibility = v;
            this.btnReplace.Visibility = v;
            this.btnReplaceAll.Visibility = v;

            switch (v)
            {
                case Visibility.Collapsed:
                case Visibility.Hidden:
                    this.Title = "Find";
                    break;
                default:
                    this.Title = "Find and Replace";
                    break;
            }
            this.stkDialog.UpdateLayout();
        }

        private void btnFindNext_Click(object sender, RoutedEventArgs e)
        {
            this._engine.FindText = this.cboFind.Text;
            this._engine.Find(this.Editor);
        }

        private void btnReplace_Click(object sender, RoutedEventArgs e)
        {
            if (this._mode == FindReplaceMode.Find) return;
            this._engine.FindText = this.cboFind.Text;
            this._engine.ReplaceText = this.cboReplace.Text;
            this._engine.Replace(this.Editor);
        }

        private void btnReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            if (this._mode == FindReplaceMode.Find) return;
            this._engine.FindText = this.cboFind.Text;
            this._engine.ReplaceText = this.cboReplace.Text;
            this._engine.ReplaceAll(this.Editor);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }

        private void btnReplace_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.btnReplace.Visibility != Visibility.Visible)
            {
                this.cboFind.Focus();
            }
        }

        private void btnReplaceAll_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.btnReplaceAll.Visibility != Visibility.Visible)
            {
                this.cboFind.Focus();
            }
        }

        private void cboReplace_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.cboReplace.Visibility != Visibility.Visible)
            {
                this.chkMatchCase.Focus();
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is EditorWindow)
            {
                this.cboFind.Focus();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cboFind.Focus();
        }

        private void cboLookIn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String tag = ((ComboBoxItem)this.cboLookIn.SelectedItem).Tag as String;
            if (tag == null)
            {
                this._engine.Scope = FindAndReplaceScope.CurrentDocument;
            }
            else
            {
                switch (tag)
                {
                    case "Selection":
                        this._engine.Scope = FindAndReplaceScope.Selection;
                        break;

                    case "Current Document":
                    default:
                        this._engine.Scope = FindAndReplaceScope.CurrentDocument;
                        break;
                }
            }
        }

        private void chkMatchCase_Click(object sender, RoutedEventArgs e)
        {
            this._engine.MatchCase = (this.chkMatchCase.IsChecked == true);
        }

        private void chkMatchWholeWord_Click(object sender, RoutedEventArgs e)
        {
            this._engine.MatchWholeWord = (this.chkMatchWholeWord.IsChecked == true);
        }

        private void chkSearchUp_Click(object sender, RoutedEventArgs e)
        {
            this._engine.SearchUp = (this.chkSearchUp.IsChecked == true);
        }

        private void chkRegex_Click(object sender, RoutedEventArgs e)
        {
            this._engine.UseRegex = (this.chkRegex.IsChecked == true);
        }
    }
}
