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
using System.IO;
using Newtonsoft.Json;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// A subclass of <see cref="JsonTextReader">JsonTextReader</see> which automatically ignores all comments
    /// </summary>
    class CommentIgnoringJsonTextReader : JsonTextReader
    {
        public CommentIgnoringJsonTextReader(TextReader reader) : base(reader) { }

        /// <summary>
        /// Reads the next non-comment Token if one is available
        /// </summary>
        /// <returns>True if a Token was read, False otherwise</returns>
        public override bool Read()
        {
            //Read next token
            bool result = base.Read();

            if (result)
            {
                //Keep reading next Token while Token is a Comment
                while (base.TokenType == JsonToken.Comment)
                {
                    result = base.Read();

                    //If we hit end of stream return false
                    if (!result) return false;
                }

                //If we get here we've read a Token which isn't a comment
                return true;
            }
            else
            {
                //Couldn't read to start with as already at end of stream
                return false;
            }
        }
    }
}
