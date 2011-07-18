using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using VDS.RDF.Utilities.Editor.WinForms.Syntax;

namespace VDS.RDF.Utilities.Editor.WinForms
{
    public static class WinFormsHighlightingManager
    {
        public static void Initialise()
        {
            HighlightingManager.Manager.AddSyntaxModeFileProvider(new RdfSyntaxModeProvider());
        }
    }
}
