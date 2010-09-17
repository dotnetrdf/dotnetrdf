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
