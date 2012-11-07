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
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    public abstract class BaseAutoCompleter<T> 
        : IAutoCompleter<T>
    {
        private AutoCompleteState _state = AutoCompleteState.None;
        private AutoCompleteState _lastCompletion = AutoCompleteState.None;
        private AutoCompleteState _temp = AutoCompleteState.None;

        protected ITextEditorAdaptor<T> _editor;
        private int _startOffset = 0;

        public BaseAutoCompleter(ITextEditorAdaptor<T> editor)
        {
            this._editor = editor;
        }

        #region Current Text Management

        public int StartOffset
        {
            get
            {
                return this._startOffset;
            }
            set
            {
                this._startOffset = value;
            }
        }

        public int Length
        {
            get
            {
                return this._editor.CaretOffset - this.StartOffset;
            }
        }

        public String CurrentText
        {
            get
            {
                if (this._editor == null)
                {
                    return String.Empty;
                }
                else
                {
                    return this._editor.GetText(this._startOffset, this.Length);
                }
            }
        }

        #endregion

        public void DetectState()
        {
            //Don't do anything if currently disabled
            if (this.State == AutoCompleteState.Disabled) return;

            //Then call the derived classes internal state detection (if any)
            this.DetectStateInternal();

            //TODO: Want to call some method in ITextEditorAdaptor which tells it what syntax element we are currently in
        }

        protected virtual void DetectStateInternal()
        { }

        public abstract void TryAutoComplete(String newText);

        public AutoCompleteState State
        {
            get
            {
                return this._state;
            }
            set
            {
                this._state = value;
            }
        }

        public AutoCompleteState LastCompletion
        {
            get
            {
                return this._lastCompletion;
            }
            set
            {
                this._lastCompletion = value;
            }
        }

        protected AutoCompleteState TemporaryState
        {
            get
            {
                return this._temp;
            }
            set
            {
                this._temp = value;
            }
        }
    }
}
