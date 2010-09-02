using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdfEditor.Syntax
{
    public class SyntaxValidationResults : ISyntaxValidationResults
    {
        private bool _valid;
        private String _message;
        private List<String> _warnings = new List<string>();
        private Exception _error;
        private Object _result;

        public SyntaxValidationResults(bool valid, String message)
        {
            this._valid = valid;
            this._message = message;
        }

        public SyntaxValidationResults(bool valid, String message, Object result)
            : this(valid, message)
        {
            this._result = result;
        }

        public SyntaxValidationResults(bool valid, String message, Object result, IEnumerable<String> warnings)
            : this(valid, message, result)
        {
            this._warnings.AddRange(warnings);
        }

        public SyntaxValidationResults(bool valid, String message, Object result, IEnumerable<String> warnings, Exception error)
            : this(valid, message, result, warnings)
        {
            this._error = error;
        }

        public SyntaxValidationResults(bool valid, String message, Exception error)
            : this(valid, message)
        {
            this._error = error;
        }

        public SyntaxValidationResults(String message, Exception error)
            : this(error == null, message, error) { }

        public bool IsValid
        {
            get 
            {
                return this._valid; 
            }
        }

        public string Message
        {
            get 
            {
                return this._message;
            }
        }

        public IEnumerable<string> Warnings
        {
            get 
            {
                return this._warnings;
            }
        }

        public Exception Error
        {
            get 
            {
                return this._error; 
            }
        }

        public Object Result
        {
            get
            {
                return this._result;
            }
        }
    }
}
