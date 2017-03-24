/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Utilities.Editor.Selection;

namespace VDS.RDF.Utilities.Editor
{
    /// <summary>
    /// Static Class containing global options for editors
    /// </summary>
    public static class GlobalOptions
    {
        private static bool _useBomForUtf8 = false;

        /// <summary>
        /// Gets/Sets whether to use the BOM on UTF-8 output
        /// </summary>
        public static bool UseBomForUtf8
        {
            get
            {
                return _useBomForUtf8;
            }
            set
            {
                _useBomForUtf8 = value;
            }
        }
    }

    /// <summary>
    /// Document Manager options
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class ManagerOptions<T>
    {
        //Syntax Highlighting
        private bool _syntaxHighlightingEnabled = true;

        //Selection
        private bool _symbolSelectEnabled = true;
        private ISymbolSelector<T> _selector = new DefaultSelector<T>();
        private bool _includeDelim = false;

        //Validation
        private bool _validateAsYouType = true;
        private bool _highlightErrors = true;

        //Auto-Complete
        private bool _autoCompleteEnabled = true;

        #region Syntax Highlighting

        /// <summary>
        /// Gets/Sets whether syntax highlighting is enabled
        /// </summary>
        public bool IsSyntaxHighlightingEnabled
        {
            get
            {
                return this._syntaxHighlightingEnabled;
            }
            set
            {
                if (value != this._syntaxHighlightingEnabled)
                {
                    this._syntaxHighlightingEnabled = value;
                    this.RaiseEvent(this.HighlightingToggled);
                }
            }
        }

        #endregion

        #region Selection

        /// <summary>
        /// Gets/Sets whether Symbol Selection is enabled
        /// </summary>
        public bool IsSymbolSelectionEnabled
        {
            get
            {
                return this._symbolSelectEnabled;
            }
            set
            {
                if (this._symbolSelectEnabled != value)
                {
                    this._symbolSelectEnabled = value;
                    this.RaiseEvent(this.SymbolSelectionToggled);
                }
            }
        }

        /// <summary>
        /// Gets/Sets whether Symbol Boundaries are included when using Symbol Selection
        /// </summary>
        public bool IncludeBoundaryInSymbolSelection
        {
            get
            {
                if (this._selector != null)
                {
                    return this._selector.IncludeDeliminator;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                this._includeDelim = value;
                if (this._selector != null)
                {
                    this._selector.IncludeDeliminator = value;
                }
            }
        }

        /// <summary>
        /// Gets the Current Symbol Selector
        /// </summary>
        public ISymbolSelector<T> CurrentSymbolSelector
        {
            get
            {
                return this._selector;
            }
            set
            {
                if (!ReferenceEquals(this._selector, value))
                {
                    this._selector = value;
                    this._selector.IncludeDeliminator = this.IncludeBoundaryInSymbolSelection;
                    this.RaiseEvent(this.SymbolSelectorChanged);
                }
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Gets/Sets whether Validate as you Type is enabled
        /// </summary>
        public bool IsValidateAsYouTypeEnabled
        {
            get
            {
                return this._validateAsYouType;
            }
            set
            {
                this._validateAsYouType = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Error Highlighting is enabled
        /// </summary>
        public bool IsHighlightErrorsEnabled
        {
            get
            {
                return this._highlightErrors;
            }
            set
            {
                if (value != this._highlightErrors)
                {
                    this._highlightErrors = value;
                    this.RaiseEvent(this.HighlightErrorsToggled);
                }
            }
        }

        #endregion

        #region Auto-Completion

        /// <summary>
        /// Gets/Sets whether Auto-Completion is enabled
        /// </summary>
        public bool IsAutoCompletionEnabled
        {
            get
            {
                return this._autoCompleteEnabled;
            }
            set
            {
                if (this._autoCompleteEnabled != value)
                {
                    this._autoCompleteEnabled = value;
                    this.RaiseEvent(this.AutoCompleteToggled);
                }
            }
        }

        #endregion

        /// <summary>
        /// Event which is raised when highlighting is toggled
        /// </summary>
        public event OptionsChanged HighlightingToggled;

        /// <summary>
        /// Event which is raised when error highlighting is toggled
        /// </summary>
        public event OptionsChanged HighlightErrorsToggled;

        /// <summary>
        /// Event which is raised when auto-completion is toggled
        /// </summary>
        public event OptionsChanged AutoCompleteToggled;

        /// <summary>
        /// Event which is raised when symbol selection is toggled
        /// </summary>
        public event OptionsChanged SymbolSelectionToggled;

        /// <summary>
        /// Event which is raised when the symbol selector is changed
        /// </summary>
        public event OptionsChanged SymbolSelectorChanged;

        private void RaiseEvent(OptionsChanged evt)
        {
            if (evt != null) evt();
        }
    }
}
