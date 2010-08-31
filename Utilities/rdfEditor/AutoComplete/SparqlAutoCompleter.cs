using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing; 

namespace rdfEditor.AutoComplete
{
    public class SparqlAutoCompleter : TurtleAutoCompleter
    {
        public SparqlAutoCompleter(SparqlQuerySyntax syntax)
        {
            foreach (String keyword in SparqlSpecsHelper.SparqlQuery10Keywords)
            {
                this._keywords.Add(new KeywordCompletionData(keyword));
            }

            if (syntax != SparqlQuerySyntax.Sparql_1_0)
            {
                //TODO: Add SPARQL 1.1 Keywords to list
            }
        }

        protected override void StartAutoComplete(TextEditor editor, TextCompositionEventArgs e)
        {
            //Only do something if auto-complete not active
            if (this.State != AutoCompleteState.None) return;

            this._editor = editor;

            if (e.Text.Length == 1)
            {
                char c = e.Text[0];
                if (Char.IsLetter(c))
                {
                    StartKeywordOrQNameCompletion(editor, e);
                }
                else if (c == '_')
                {
                    StartBNodeCompletion(editor, e);
                }
                else if (c == ':')
                {
                    StartQNameCompletion(editor, e);
                }
                else if (c == '<')
                {
                    StartUriCompletion(editor, e);
                }
                else if (c == '#')
                {
                    StartCommentCompletion(editor, e);
                }
                else if (c == '"')
                {
                    StartLiteralCompletion(editor, e);
                }
                else if (c == '.' || c == ',' || c == ';')
                {
                    this.State = AutoCompleteState.None;
                }
            }

            if (this.State == AutoCompleteState.None || this.State == AutoCompleteState.Disabled) return;

            //If no completion window in use have to manually set the Start Offset and Length
            if (this._c == null)
            {
                this.StartOffset = editor.CaretOffset - 1;
            }
        }
    }
}
