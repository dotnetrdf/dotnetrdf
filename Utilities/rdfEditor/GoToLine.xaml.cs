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

namespace rdfEditor
{
    /// <summary>
    /// Interaction logic for GoToLine.xaml
    /// </summary>
    public partial class GoToLine : Window
    {
        private int _line, _maxLine;

        public GoToLine(TextEditor editor)
        {
            InitializeComponent();

            this._line = editor.Document.GetLineByOffset(editor.CaretOffset).LineNumber;
            this._maxLine = editor.Document.LineCount;
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
