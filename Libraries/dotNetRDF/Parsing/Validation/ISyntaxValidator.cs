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

namespace VDS.RDF.Parsing.Validation
{
    /// <summary>
    /// Interface for classes which can validate Syntax
    /// </summary>
    public interface ISyntaxValidator
    {
        /// <summary>
        /// Validates the given Data
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns></returns>
        ISyntaxValidationResults Validate(String data);
    }

    /// <summary>
    /// Interface for Validation Results
    /// </summary>
    public interface ISyntaxValidationResults
    {
        /// <summary>
        /// Gets whether the Syntax was valid
        /// </summary>
        bool IsValid
        {
            get;
        }

        /// <summary>
        /// Gets an informational message about the validity/invalidity of the Syntax
        /// </summary>
        String Message
        {
            get;
        }

        /// <summary>
        /// Gets an enumeration of any warning messages
        /// </summary>
        IEnumerable<String> Warnings
        {
            get;
        }

        /// <summary>
        /// Gets any validation error
        /// </summary>
        Exception Error
        {
            get;
        }

        /// <summary>
        /// Gets any result object that was parsed from the syntax
        /// </summary>
        Object Result
        {
            get;
        }
    }
}
