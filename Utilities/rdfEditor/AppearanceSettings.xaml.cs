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
using ICSharpCode.AvalonEdit;
using VDS.RDF.Utilities.Editor.Syntax;

namespace VDS.RDF.Utilities.Editor
{
    /// <summary>
    /// Interaction logic for AppearanceSettings.xaml
    /// </summary>
    public partial class AppearanceSettings : Window
    {
        private List<String> _colours = new List<String>();
        private TextEditor _editor;

        public AppearanceSettings(TextEditor editor)
        {
            InitializeComponent();

            this._editor = editor;

            //Prepare a list of colours as we need this to select the relevant colours later
            Type t = typeof(Colors);
            Type cType = typeof(Color);
            foreach (PropertyInfo p in t.GetProperties())
            {
                if (p.PropertyType.Equals(cType))
                {
                    try
                    {
                        Color c = (Color)ColorConverter.ConvertFromString(p.Name);
                        this._colours.Add(c.ToString());
                    }
                    catch
                    {
                        //Ignore errors here
                    }
                }
            }


            //Show current settings
            this.ShowSettings();
        }

        private void ShowSettings()
        {
            //Show Editor Settings
            FontFamily font = (Properties.Settings.Default.EditorFontFace == null) ? this._editor.FontFamily : Properties.Settings.Default.EditorFontFace;
            this.cboFont.SelectedItem = font;
            this.fontSizeSlider.Value = Math.Round(Properties.Settings.Default.EditorFontSize);
            this.txtEditorFontColor.Text = Properties.Settings.Default.EditorForeground.ToString();
            this.txtEditorBackColor.Text = Properties.Settings.Default.EditorBackground.ToString();

            //Show Syntax Highlighting Settings
            this.ShowColour(this.cboColourXmlAttrName, Properties.Settings.Default.SyntaxColourXmlAttrName);
            this.ShowColour(this.cboColourXmlAttrValue, Properties.Settings.Default.SyntaxColourXmlAttrValue);
            this.ShowColour(this.cboColourXmlBrokenEntities, Properties.Settings.Default.SyntaxColourXmlBrokenEntity);
            this.ShowColour(this.cboColourXmlCData, Properties.Settings.Default.SyntaxColourXmlCData);
            this.ShowColour(this.cboColourXmlComments, Properties.Settings.Default.SyntaxColourXmlComments);
            this.ShowColour(this.cboColourXmlDocType, Properties.Settings.Default.SyntaxColourXmlDocType);
            this.ShowColour(this.cboColourXmlEntities, Properties.Settings.Default.SyntaxColourXmlEntity);
            this.ShowColour(this.cboColourXmlTags, Properties.Settings.Default.SyntaxColourXmlTag);

            this.ShowColour(this.cboColourBNode, Properties.Settings.Default.SyntaxColourBNode);
            this.ShowColour(this.cboColourComments, Properties.Settings.Default.SyntaxColourComment);
            this.ShowColour(this.cboColourEscapedChars, Properties.Settings.Default.SyntaxColourEscapedChar);
            this.ShowColour(this.cboColourKeywords, Properties.Settings.Default.SyntaxColourKeyword);
            this.ShowColour(this.cboColourLangSpec, Properties.Settings.Default.SyntaxColourLangSpec);
            this.ShowColour(this.cboColourNumbers, Properties.Settings.Default.SyntaxColourNumbers);
            this.ShowColour(this.cboColourPunctuation, Properties.Settings.Default.SyntaxColourPunctuation);
            this.ShowColour(this.cboColourQNames, Properties.Settings.Default.SyntaxColourQName);
            this.ShowColour(this.cboColourStrings, Properties.Settings.Default.SyntaxColourString);
            this.ShowColour(this.cboColourURIs, Properties.Settings.Default.SyntaxColourURI);
            this.ShowColour(this.cboColourVariable, Properties.Settings.Default.SyntaxColourVariables);
        }

        private void ShowColour(ComboBox combo, Color c)
        {
            String s = c.ToString();
            for (int i = 0; i < this._colours.Count; i++)
            {
                if (s.Equals(this._colours[i]))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //First save general Editor Appearance Settings
                Properties.Settings.Default.EditorFontFace = (FontFamily)this.cboFont.SelectedItem;
                Properties.Settings.Default.EditorFontSize = this.fontSizeSlider.Value;
                Properties.Settings.Default.EditorForeground = (Color)ColorConverter.ConvertFromString(this.txtEditorFontColor.Text);
                Properties.Settings.Default.EditorBackground = (Color)ColorConverter.ConvertFromString(this.txtEditorBackColor.Text);
                Properties.Settings.Default.Save();

                //Then save Syntax Highlighting Settings
                Properties.Settings.Default.SyntaxColourXmlAttrName = this.GetColour(Properties.Settings.Default.SyntaxColourXmlAttrName, this.cboColourXmlAttrName);
                Properties.Settings.Default.SyntaxColourXmlAttrValue = this.GetColour(Properties.Settings.Default.SyntaxColourXmlAttrValue, this.cboColourXmlAttrValue);
                Properties.Settings.Default.SyntaxColourXmlBrokenEntity = this.GetColour(Properties.Settings.Default.SyntaxColourXmlBrokenEntity, this.cboColourXmlBrokenEntities);
                Properties.Settings.Default.SyntaxColourXmlCData = this.GetColour(Properties.Settings.Default.SyntaxColourXmlCData, this.cboColourXmlCData);
                Properties.Settings.Default.SyntaxColourXmlComments = this.GetColour(Properties.Settings.Default.SyntaxColourXmlComments, this.cboColourXmlComments);
                Properties.Settings.Default.SyntaxColourXmlDocType = this.GetColour(Properties.Settings.Default.SyntaxColourXmlDocType, this.cboColourXmlDocType);
                Properties.Settings.Default.SyntaxColourXmlEntity = this.GetColour(Properties.Settings.Default.SyntaxColourXmlEntity, this.cboColourXmlEntities);
                Properties.Settings.Default.SyntaxColourXmlTag = this.GetColour(Properties.Settings.Default.SyntaxColourXmlTag, this.cboColourXmlTags);

                Properties.Settings.Default.SyntaxColourBNode = this.GetColour(Properties.Settings.Default.SyntaxColourBNode, this.cboColourBNode);
                Properties.Settings.Default.SyntaxColourComment = this.GetColour(Properties.Settings.Default.SyntaxColourComment, this.cboColourComments);
                Properties.Settings.Default.SyntaxColourEscapedChar = this.GetColour(Properties.Settings.Default.SyntaxColourEscapedChar, this.cboColourEscapedChars);
                Properties.Settings.Default.SyntaxColourKeyword = this.GetColour(Properties.Settings.Default.SyntaxColourKeyword, this.cboColourKeywords);
                Properties.Settings.Default.SyntaxColourLangSpec = this.GetColour(Properties.Settings.Default.SyntaxColourLangSpec, this.cboColourLangSpec);
                Properties.Settings.Default.SyntaxColourNumbers = this.GetColour(Properties.Settings.Default.SyntaxColourNumbers, this.cboColourNumbers);
                Properties.Settings.Default.SyntaxColourPunctuation = this.GetColour(Properties.Settings.Default.SyntaxColourPunctuation, this.cboColourPunctuation);
                Properties.Settings.Default.SyntaxColourQName = this.GetColour(Properties.Settings.Default.SyntaxColourQName, this.cboColourQNames);
                Properties.Settings.Default.SyntaxColourString = this.GetColour(Properties.Settings.Default.SyntaxColourString, this.cboColourStrings);
                Properties.Settings.Default.SyntaxColourURI = this.GetColour(Properties.Settings.Default.SyntaxColourURI, this.cboColourURIs);
                Properties.Settings.Default.SyntaxColourVariables = this.GetColour(Properties.Settings.Default.SyntaxColourVariables, this.cboColourVariable);

                //Finally save the updated settings
                Properties.Settings.Default.Save();

                //Force the Syntax Manager to update colours appropriately
                SyntaxManager.UpdateHighlightingColours();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred trying to save your settings: " + ex.Message, "Save Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Color GetColour(Color def, ComboBox combo)
        {
            try
            {
                if (combo.SelectedIndex < 0 || combo.SelectedIndex >= this._colours.Count)
                {
                    return def;
                }
                else
                {
                    return (Color)ColorConverter.ConvertFromString(this._colours[combo.SelectedIndex]);
                }
            }
            catch
            {
                return def;
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FontFamilyConverter converter = new FontFamilyConverter();
                FontFamily consolas = (FontFamily)converter.ConvertFromString("Consolas");
                Properties.Settings.Default.EditorFontFace = consolas;
                this.fontSizeSlider.Value = 13d;
                Properties.Settings.Default.EditorForeground = Colors.Black;
                Properties.Settings.Default.EditorBackground = Colors.White;
                Properties.Settings.Default.Save();
            }
            catch
            {
                //Can't reset settings
            }
            //Show settings
            this.ShowSettings();
        }

        private void lnkAdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.lnkAdvancedSettings.NavigateUri.ToString());
            }
            catch
            {
                //Ignore errors launching the URI
            }
        }

        private void btnResetSyntaxChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.SyntaxColourBNode = Colors.SteelBlue;
                Properties.Settings.Default.SyntaxColourComment = Colors.Green;
                Properties.Settings.Default.SyntaxColourEscapedChar = Colors.Teal;
                Properties.Settings.Default.SyntaxColourKeyword = Colors.Red;
                Properties.Settings.Default.SyntaxColourLangSpec = Colors.DarkGreen;
                Properties.Settings.Default.SyntaxColourNumbers = Colors.DarkBlue;
                Properties.Settings.Default.SyntaxColourPunctuation = Colors.DarkGreen;
                Properties.Settings.Default.SyntaxColourQName = Colors.DarkMagenta;
                Properties.Settings.Default.SyntaxColourString = Colors.Blue;
                Properties.Settings.Default.SyntaxColourURI = Colors.DarkMagenta;
                Properties.Settings.Default.SyntaxColourVariables = Colors.DarkOrange;
                Properties.Settings.Default.SyntaxColourXmlAttrName = Colors.Red;
                Properties.Settings.Default.SyntaxColourXmlAttrValue = Colors.Blue;
                Properties.Settings.Default.SyntaxColourXmlBrokenEntity = Colors.Olive;
                Properties.Settings.Default.SyntaxColourXmlCData = Colors.Blue;
                Properties.Settings.Default.SyntaxColourXmlComments = Colors.Green;
                Properties.Settings.Default.SyntaxColourXmlDocType = Colors.Blue;
                Properties.Settings.Default.SyntaxColourXmlEntity = Colors.Teal;
                Properties.Settings.Default.SyntaxColourXmlTag = Colors.DarkMagenta;
                Properties.Settings.Default.Save();
            }
            catch
            {
                //Can't reset settings
            }
            this.ShowSettings();
            SyntaxManager.UpdateHighlightingColours();
        }

        private void btnAbandon_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
