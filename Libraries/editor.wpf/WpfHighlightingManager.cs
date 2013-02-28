/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
    /// <summary>
    /// A highlighting manager that configures AvalonEdit highlighting
    /// </summary>
    public static class WpfHighlightingManager
    {
        private static bool _init = false;
        private static bool _useCustomSyntaxFiles = false;

        /// <summary>
        /// Initialize the manager
        /// </summary>
        public static void Initialise()
        {
            Initialise(false);
        }

        /// <summary>
        /// Initialize the manager
        /// </summary>
        /// <param name="useCustomSyntaxFiles">Whether to use custom syntax definitions in place of the embedded definitions</param>
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

        /// <summary>
        /// Attempts to load a highlighting definition from a file
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Highlighting Definition</returns>
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

        /// <summary>
        /// Attempts to load a highlighting definition optionally using the embedded resource where available
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="useResourceIfAvailable">Whether to use embedded resources</param>
        /// <returns>Highlight Definition</returns>
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
