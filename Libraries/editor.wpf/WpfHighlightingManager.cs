using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using VDS.RDF.Utilities.Editor.Syntax;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    public static class WpfHighlightingManager
    {
        private static bool _init = false;
        private static bool _useCustomSyntaxFiles = false;

        public static void Initialise()
        {
            Initialise(false);
        }

        public static void Initialise(bool useCustomSyntaxFiles)
        {
            if (!_init)
            {
                _useCustomSyntaxFiles = useCustomSyntaxFiles;

                foreach (SyntaxDefinition def in SyntaxManager.Definitions)
                {
                    if (def.DefinitionFile != null)
                    {
                        try
                        {
                            HighlightingManager.Instance.RegisterHighlighting(def.Name, def.FileExtensions, WpfHighlightingManager.LoadHighlighting(def.DefinitionFile, true));
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("Syntax Highlighting for " + def.Name + " will not be available as the Highlighting Definition was malformed:\n" + ex.Message);
                        }
                    }
                }
                _init = true;
            }
        }

        private static IHighlightingDefinition LoadHighlighting(String filename)
        {
            if (File.Exists(Path.Combine("syntax/", filename)))
            {
                return HighlightingLoader.Load(XmlReader.Create(Path.Combine("syntax/", filename)), HighlightingManager.Instance);
            }
            else
            {
                return HighlightingLoader.Load(XmlReader.Create(filename), HighlightingManager.Instance);
            }
        }

        private static IHighlightingDefinition LoadHighlighting(String filename, bool useResourceIfAvailable)
        {
            if (useResourceIfAvailable)
            {
                //If the user has specified to use customised XSHD files then we'll use the files from
                //the Syntax directory instead of the embedded resources
                if (!_useCustomSyntaxFiles)
                {

                    //Try and load it from an embedded resource
                    Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("VDS.RDF.Utilities.Editor.Wpf.Syntax." + filename);
                    if (resource != null)
                    {
                        return HighlightingLoader.Load(XmlReader.Create(resource), HighlightingManager.Instance);
                    }
                }
                else
                {
                    return LoadHighlighting(filename);
                }
            }

            //If no resource available try and load from file
            return HighlightingLoader.Load(XmlReader.Create(filename), HighlightingManager.Instance);
        }
    }
}
