using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Utilities.Editor.Selection;

namespace VDS.RDF.Utilities.Editor
{
    public class GlobalOptions<T>
    {
        //Selection
        private bool _symbolSelectEnabled = true;
        private ISymbolSelector<T> _selector = new DefaultSelector<T>();
        private bool _includeDelim = false;

        //Validation
        private bool _validateAsYouType = false;
        private bool _highlightErrors = true;

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
                this._symbolSelectEnabled = value;
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
        }

        #endregion

        #region Validation

        /// <summary>
        /// Gets/Sets whether Validate as you Type is enabled
        /// </summary>
        public bool IsValidateAsYouType
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
                this._highlightErrors = value;
            }
        }

        #endregion
    }
}
