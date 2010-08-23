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
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Formatting
{
    public class UncompressedTurtleFormatter : NTriplesFormatter
    {
        public UncompressedTurtleFormatter()
            : base("Turtle") { }

        protected UncompressedTurtleFormatter(String formatName)
            : base(formatName) { }

        public override string FormatChar(char c)
        {
            return c.ToString();
        }
    }

    public class TurtleFormatter : QNameFormatter
    {
        private BlankNodeOutputMapper _bnodeMapper = new BlankNodeOutputMapper(WriterHelper.IsValidBlankNodeID);

        public TurtleFormatter() 
            : base("Turtle", new QNameOutputMapper()) { }

        public TurtleFormatter(IGraph g)
            : base("Turtle", new QNameOutputMapper(g.NamespaceMap)) { }

        public TurtleFormatter(INamespaceMapper nsmap)
            : base("Turtle", new QNameOutputMapper(nsmap)) { }

        protected TurtleFormatter(String formatName)
            : base(formatName, new QNameOutputMapper()) { }

        protected TurtleFormatter(String formatName, IGraph g)
            : base(formatName, new QNameOutputMapper(g.NamespaceMap)) { }

        protected TurtleFormatter(String formatName, INamespaceMapper nsmap)
            : base(formatName, new QNameOutputMapper(nsmap)) { }

        protected override string FormatLiteralNode(LiteralNode l)
        {
            StringBuilder output = new StringBuilder();
            String value, qname;
            bool longlit = false, plainlit = false;

            longlit = TurtleSpecsHelper.IsLongLiteral(l.Value);
            plainlit = TurtleSpecsHelper.IsValidPlainLiteral(l.Value, l.DataType);

            if (plainlit)
            {
                output.Append(l.Value);
                if (TurtleSpecsHelper.IsValidInteger(l.Value))
                {
                    output.Append(' ');
                }
            }
            else
            {
                output.Append('"');
                if (longlit) output.Append("\"\"");

                value = l.Value;
                //This first replace escapes all back slashes for good measure
                if (value.Contains("\\")) value = value.Replace("\\", "\\\\");

                //Then remove null character since it doesn't change the meaning of the Literal
                value = value.Replace("\0", "");

                //Don't need all the other escapes for long literals as the characters that would be escaped are permitted in long literals
                //Need to escape " still
                value = value.Replace("\"", "\\\"");

                if (!longlit)
                {
                    //Then if we're not a long literal we'll escape tabs
                    value = value.Replace("\t", "\\t");
                }
                output.Append(value);
                output.Append('"');
                if (longlit) output.Append("\"\"");

                if (!l.Language.Equals(String.Empty))
                {
                    output.Append('@');
                    output.Append(l.Language);
                }
                else if (l.DataType != null)
                {
                    output.Append("^^");
                    if (this._qnameMapper.ReduceToQName(l.DataType.ToString(), out qname))
                    {
                        if (TurtleSpecsHelper.IsValidQName(qname))
                        {
                            output.Append(qname);
                        }
                        else
                        {
                            output.Append('<');
                            output.Append(this.FormatUri(l.DataType));
                            output.Append('>');
                        }
                    }
                    else
                    {
                        output.Append('<');
                        output.Append(this.FormatUri(l.DataType));
                        output.Append('>');
                    }
                }
            }

            return output.ToString();
        }

        protected override string FormatBlankNode(BlankNode b)
        {
            return "_:" + this._bnodeMapper.GetOutputID(b.InternalID);
        }
    }
}
