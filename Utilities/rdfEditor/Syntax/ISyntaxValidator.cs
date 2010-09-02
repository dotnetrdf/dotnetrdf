using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdfEditor.Syntax
{
    public interface ISyntaxValidator
    {
        ISyntaxValidationResults Validate(String data);
    }

    public interface ISyntaxValidationResults
    {
        bool IsValid
        {
            get;
        }

        String Message
        {
            get;
        }

        IEnumerable<String> Warnings
        {
            get;
        }

        Exception Error
        {
            get;
        }

        Object Result
        {
            get;
        }
    }
}
