/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

namespace VDS.RDF.Parsing.Validation
{
    /// <summary>
    /// Represents Syntax Validation Results
    /// </summary>
    public class SyntaxValidationResults : ISyntaxValidationResults
    {
        private bool _valid;
        private String _message;
        private List<String> _warnings = new List<string>();
        private Exception _error;
        private Object _result;

        /// <summary>
        /// Creates new Syntax Validation Results
        /// </summary>
        /// <param name="valid">Whether the Syntax was valid</param>
        /// <param name="message">Validation Message</param>
        public SyntaxValidationResults(bool valid, String message)
        {
            this._valid = valid;
            this._message = message;
        }

        /// <summary>
        /// Creates new Syntax Validation Results
        /// </summary>
        /// <param name="valid">Whether the Syntax was valid</param>
        /// <param name="message">Validation Message</param>
        /// <param name="result">Results Object</param>
        public SyntaxValidationResults(bool valid, String message, Object result)
            : this(valid, message)
        {
            this._result = result;
        }

        /// <summary>
        /// Creates new Syntax Validation Results
        /// </summary>
        /// <param name="valid">Whether the Syntax was valid</param>
        /// <param name="message">Validation Message</param>
        /// <param name="result">Results Object</param>
        /// <param name="warnings">Enumeration of Warnings</param>
        public SyntaxValidationResults(bool valid, String message, Object result, IEnumerable<String> warnings)
            : this(valid, message, result)
        {
            this._warnings.AddRange(warnings);
        }

        /// <summary>
        /// Creates new Syntax Validation Results
        /// </summary>
        /// <param name="valid">Whether the Syntax was valid</param>
        /// <param name="message">Validation Message</param>
        /// <param name="result">Results Object</param>
        /// <param name="warnings">Enumeration of Warnings</param>
        /// <param name="error">Error that occurred</param>
        public SyntaxValidationResults(bool valid, String message, Object result, IEnumerable<String> warnings, Exception error)
            : this(valid, message, result, warnings)
        {
            this._error = error;
        }

        /// <summary>
        /// Creates new Syntax Validation Results
        /// </summary>
        /// <param name="valid">Whether the Syntax was valid</param>
        /// <param name="message">Validation Message</param>
        /// <param name="error">Error that occurred</param>
        public SyntaxValidationResults(bool valid, String message, Exception error)
            : this(valid, message)
        {
            this._error = error;
        }

        /// <summary>
        /// Creates new Syntax Validation Results
        /// </summary>
        /// <param name="message">Validation Message</param>
        /// <param name="error">Error that occurred</param>
        public SyntaxValidationResults(String message, Exception error)
            : this(error == null, message, error) { }

        /// <summary>
        /// Whether the Syntax was valid
        /// </summary>
        public bool IsValid
        {
            get 
            {
                return this._valid; 
            }
        }

        /// <summary>
        /// Gets the Validation Message
        /// </summary>
        public string Message
        {
            get 
            {
                return this._message;
            }
        }

        /// <summary>
        /// Gets the Warnings that were produced
        /// </summary>
        public IEnumerable<string> Warnings
        {
            get 
            {
                return this._warnings;
            }
        }

        /// <summary>
        /// Gets the Error that occurred
        /// </summary>
        public Exception Error
        {
            get 
            {
                return this._error; 
            }
        }

        /// <summary>
        /// Gets the Result Object that was produced
        /// </summary>
        public Object Result
        {
            get
            {
                return this._result;
            }
        }
    }
}
