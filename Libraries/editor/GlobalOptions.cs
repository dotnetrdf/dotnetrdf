using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public class GlobalOptions
    {
        //Selection
        private bool _symbolSelectEnabled = true;
        private ISymbolSelector _selector = new DefaultSelector();
        private bool _includeDelim = false;

        //Validation
        private bool _validateAsYouType = false;
        private bool _highlightErrors = true;

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
    }
}
