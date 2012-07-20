/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Utilities.Editor.Selection;

namespace VDS.RDF.Utilities.Editor
{
    public static class GlobalOptions
    {
        private static bool _useBomForUtf8 = false;

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

        public event OptionsChanged HighlightingToggled;

        public event OptionsChanged HighlightErrorsToggled;

        public event OptionsChanged AutoCompleteToggled;

        public event OptionsChanged SymbolSelectionToggled;

        public event OptionsChanged SymbolSelectorChanged;

        private void RaiseEvent(OptionsChanged evt)
        {
            if (evt != null) evt();
        }
    }
}
