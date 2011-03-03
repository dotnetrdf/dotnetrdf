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

namespace VDS.RDF.Utilities.Editor
{
    /// <summary>
    /// Interaction logic for AppearanceSettings.xaml
    /// </summary>
    public partial class AppearanceSettings : Window
    {
        public AppearanceSettings(TextEditor editor)
        {
            InitializeComponent();

            FontFamily font = (Properties.Settings.Default.EditorFontFace == null) ? editor.FontFamily : Properties.Settings.Default.EditorFontFace;
            this.cboFont.SelectedItem = font;
            this.fontSizeSlider.Value = Math.Round(Properties.Settings.Default.EditorFontSize);
            this.txtEditorFontColor.Text = Properties.Settings.Default.EditorForeground.ToString();
            this.txtEditorBackColor.Text = Properties.Settings.Default.EditorBackground.ToString();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.EditorFontFace = (FontFamily)this.cboFont.SelectedItem;
                Properties.Settings.Default.EditorFontSize = this.fontSizeSlider.Value;
                Properties.Settings.Default.EditorForeground = (Color)ColorConverter.ConvertFromString(this.txtEditorFontColor.Text);
                Properties.Settings.Default.EditorBackground = (Color)ColorConverter.ConvertFromString(this.txtEditorBackColor.Text);
                Properties.Settings.Default.Save();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred trying to save your settings: " + ex.Message, "Save Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FontFamilyConverter converter = new FontFamilyConverter();
                FontFamily consolas = (FontFamily)converter.ConvertFromString("Consolas");
                this.cboFont.SelectedItem = consolas;
            }
            catch
            {
                //Can't reset font family
            }
            this.fontSizeSlider.Value = 13;
            this.txtEditorFontColor.Text = "#FF000000";
            this.txtEditorBackColor.Text = "#FFFFFFFF";

        }
    }
}
