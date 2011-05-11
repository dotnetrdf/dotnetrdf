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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Helper class with useful constants relating to the RDF Specification
    /// </summary>
    public class RdfSpecsHelper
    {
        /// <summary>
        /// URI for rdf:first
        /// </summary>
        public const String RdfListFirst = "http://www.w3.org/1999/02/22-rdf-syntax-ns#first";
        /// <summary>
        /// URI for rdf:rest
        /// </summary>
        public const String RdfListRest = "http://www.w3.org/1999/02/22-rdf-syntax-ns#rest";
        /// <summary>
        /// URI for rdf:nil
        /// </summary>
        public const String RdfListNil = "http://www.w3.org/1999/02/22-rdf-syntax-ns#nil";
        /// <summary>
        /// URI for rdf:type
        /// </summary>
        public const String RdfType = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";
        /// <summary>
        /// URI for rdf:XMLLiteral
        /// </summary>
        public const String RdfXmlLiteral = "http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral";
        /// <summary>
        /// URI for rdf:subject
        /// </summary>
        public const String RdfSubject = "http://www.w3.org/1999/02/22-rdf-syntax-ns#subject";
        /// <summary>
        /// URI for rdf:predicate
        /// </summary>
        public const String RdfPredicate = "http://www.w3.org/1999/02/22-rdf-syntax-ns#predicate";
        /// <summary>
        /// URI for rdf:object
        /// </summary>
        public const String RdfObject = "http://www.w3.org/1999/02/22-rdf-syntax-ns#object";
        /// <summary>
        /// URI for rdf:Statement
        /// </summary>
        public const String RdfStatement = "http://www.w3.org/1999/02/22-rdf-syntax-ns#Statement";

        /// <summary>
        /// Pattern for Valid Language Specifiers
        /// </summary>
        public const String ValidLangSpecifiersPattern = "^@?[A-Za-z]+(\\-[A-Za-z0-9]+)*$";

        /// <summary>
        /// Regular Expression for Valid Language Specifiers
        /// </summary>
        private static Regex _validLangSpecifier = new Regex(ValidLangSpecifiersPattern);

        /// <summary>
        /// Determines whether a given String is a valid Language Specifier
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns></returns>
        public static bool IsValidLangSpecifier(String value)
        {
            return _validLangSpecifier.IsMatch(value);
        }
    }
}
