using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using Data = VDS.RDF.Utilities.Editor.AutoComplete.Data;

namespace VDS.RDF.Utilities.Editor.WinForms.AutoComplete
{
    public class WinFormsCompletionProvider : ICompletionDataProvider
    {
        public int DefaultIndex
        {
            get 
            {
                return 0; 
            }
        }

        public ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
        {
            throw new NotImplementedException();
        }

        public System.Windows.Forms.ImageList ImageList
        {
            get 
            {
                return null; 
            }
        }

        public bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
        {
            throw new NotImplementedException();
        }

        public string PreSelection
        {
            get 
            {
                return String.Empty; 
            }
        }

        public CompletionDataProviderKeyResult ProcessKey(char key)
        {
            return CompletionDataProviderKeyResult.NormalKey;
        }
    }
}
