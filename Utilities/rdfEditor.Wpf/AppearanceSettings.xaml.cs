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
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using VDS.RDF.Utilities.Editor.Syntax;
using VDS.RDF.Utilities.Editor.Wpf.Syntax;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Interaction logic for AppearanceSettings.xaml
    /// </summary>
    public partial class AppearanceSettings
        : Window
    {
        private List<String> _colours = new List<String>();
        private VisualOptions<FontFamily, Color> _options;

        private List<String> _decorations;

        private static ITextRunConstructionContext _colourContext = new FakeTextRunContext();

        public AppearanceSettings(VisualOptions<FontFamily, Color> options)
        {
            this._options = options;

            this._decorations = new List<string>()
            {
                "None",
                "Baseline",
                "OverLine",
                "Strikethrough",
                "Underline"
            };
            InitializeComponent();

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

        #region Display Settings

        public IEnumerable<String> Decorations
        {
            get
            {
                return this._decorations;
            }
        }

        private void ShowSettings()
        {
            //Show Editor Settings
            FontFamily font = (Properties.Settings.Default.EditorFontFace == null) ? this._options.FontFace : Properties.Settings.Default.EditorFontFace;
            this.cboFont.SelectedItem = font;
            this.fontSizeSlider.Value = Math.Round(Properties.Settings.Default.EditorFontSize);
            this.ShowColour(this.cboEditorForeground, Properties.Settings.Default.EditorForeground);
            this.ShowColour(this.cboEditorBackground, Properties.Settings.Default.EditorBackground);

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

            //Show Error Highlighting Settings
            font = (Properties.Settings.Default.EditorFontFace == null) ? this._options.FontFace : Properties.Settings.Default.ErrorHighlightFontFamily;
            this.cboErrorFont.SelectedItem = font;
            this.ShowDecoration(this.cboErrorDecoration, Properties.Settings.Default.ErrorHighlightDecoration);
            this.ShowColour(this.cboColourErrorFont, Properties.Settings.Default.ErrorHighlightForeground);
            this.ShowColour(this.cboColourErrorBackground, Properties.Settings.Default.ErrorHighlightBackground);
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

        private void ShowDecoration(ComboBox combo, String decoration)
        {
            if (decoration == null || decoration.Equals(String.Empty))
            {
                decoration = "None";
            }
            for (int i = 0; i < this._decorations.Count; i++)
            {
                if (decoration.Equals(this._decorations[i]))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private String GetDecoration(ComboBox combo)
        {
            if (combo.SelectedItem == null || combo.SelectedIndex < 0)
            {
                return null;
            }
            else
            {
                return _decorations[combo.SelectedIndex];
            }
        }

        #endregion

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //First save general Editor Appearance Settings
                Properties.Settings.Default.EditorFontFace = (FontFamily)this.cboFont.SelectedItem;
                Properties.Settings.Default.EditorFontSize = this.fontSizeSlider.Value;
                Properties.Settings.Default.EditorForeground = this.GetColour(Properties.Settings.Default.EditorForeground, this.cboEditorForeground);
                Properties.Settings.Default.EditorBackground = this.GetColour(Properties.Settings.Default.EditorBackground, this.cboEditorBackground);
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

                //Then save the Error Highlighting Settings
                Properties.Settings.Default.ErrorHighlightBackground = this.GetColour(Properties.Settings.Default.ErrorHighlightBackground, this.cboColourErrorBackground);
                Properties.Settings.Default.ErrorHighlightDecoration = this.GetDecoration(this.cboErrorDecoration);
                Properties.Settings.Default.ErrorHighlightFontFamily = (FontFamily)this.cboErrorFont.SelectedItem;
                Properties.Settings.Default.ErrorHighlightForeground = this.GetColour(Properties.Settings.Default.ErrorHighlightForeground, this.cboColourErrorFont);

                //Finally save the updated settings
                Properties.Settings.Default.Save();

                //Force the Syntax Manager to update colours appropriately
                AppearanceSettings.UpdateHighlightingColours();

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
                System.Diagnostics.Process.Start(this.lnkAdvancedSettings.NavigateUri.AbsoluteUri);
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
            //SyntaxManager.UpdateHighlightingColours();
        }

        private void btnAbandon_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnErrorReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.ErrorHighlightBackground = Colors.DarkRed;
                Properties.Settings.Default.ErrorHighlightForeground = Colors.White;
                Properties.Settings.Default.ErrorHighlightDecoration = null;
                Properties.Settings.Default.ErrorHighlightFontFamily = null;
            }
            catch
            {
                //Can't reset settings
            }
            this.ShowSettings();
        }

        private void btnResetAll_Click(object sender, RoutedEventArgs e)
        {
            btnReset_Click(sender, e);
            btnResetSyntaxChanges_Click(sender, e);
            btnErrorReset_Click(sender, e);
        }

        public static void UpdateHighlightingColours()
        {
            //Only applicable if not using customised XSHD Files
            if (!Properties.Settings.Default.UseCustomisedXshdFiles)
            {
                IHighlightingDefinition h;

                //Apply XML Format Colours
                h = HighlightingManager.Instance.GetDefinition("XML");
                if (h != null)
                {
                    foreach (HighlightingColor c in h.NamedHighlightingColors)
                    {
                        switch (c.Name)
                        {
                            case "Comment":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlComments);
                                break;
                            case "CData":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlCData);
                                break;
                            case "DocType":
                            case "XmlDeclaration":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlDocType);
                                break;
                            case "XmlTag":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlTag);
                                break;
                            case "AttributeName":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlAttrName);
                                break;
                            case "AttributeValue":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlAttrValue);
                                break;
                            case "Entity":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlEntity);
                                break;
                            case "BrokenEntity":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlBrokenEntity);
                                break;

                        }
                    }
                }

                //Apply non-XML format colours
                foreach (SyntaxDefinition def in SyntaxManager.Definitions)
                {
                    if (!def.IsXmlFormat)
                    {
                        h = HighlightingManager.Instance.GetDefinition(def.Name);
                        if (h != null)
                        {
                            foreach (HighlightingColor c in h.NamedHighlightingColors)
                            {
                                switch (c.Name)
                                {
                                    case "BNode":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourBNode);
                                        break;
                                    case "Comment":
                                    case "Comments":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourComment);
                                        break;
                                    case "EscapedChar":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourEscapedChar);
                                        break;
                                    case "Keyword":
                                    case "Keywords":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourKeyword);
                                        break;
                                    case "LangSpec":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourLangSpec);
                                        break;
                                    case "Numbers":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourNumbers);
                                        break;
                                    case "Punctuation":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourPunctuation);
                                        break;
                                    case "QName":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourQName);
                                        break;
                                    case "String":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourString);
                                        break;
                                    case "URI":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourURI);
                                        break;
                                    case "Variable":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourVariables);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void AdjustHighlightingColour(HighlightingColor current, Color desired)
        {
            if (!desired.Equals(current.Foreground.GetColor(_colourContext)))
            {
                current.Foreground = new CustomHighlightingBrush(desired);
            }
        }
    }
}
