using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Formatting
{
    public abstract class DeliminatedLineFormatter : BaseFormatter
    {
        private Nullable<char> _uriStartChar, _uriEndChar, _literalWrapperChar, _longLiteralWrapperChar, _lineEndChar;
        private char _deliminatorChar = ' ';
        private char _escapeChar = '\\';
        private bool _fullLiteralOutput = true;

        public DeliminatedLineFormatter(String formatName, char deliminator, char escape, Nullable<char> uriStartChar, Nullable<char> uriEndChar, Nullable<char> literalWrapperChar, Nullable<char> longLiteralWrapperChar, Nullable<char> lineEndChar, bool fullLiteralOutput)
            : base(formatName)
        {
            this._deliminatorChar = deliminator;
            this._escapeChar = escape;
            this._uriStartChar = uriStartChar;
            this._uriEndChar = uriEndChar;
            this._literalWrapperChar = literalWrapperChar;
            this._longLiteralWrapperChar = longLiteralWrapperChar;
            this._lineEndChar = lineEndChar;
            this._fullLiteralOutput = fullLiteralOutput;
        }

        public override string Format(Triple t)
        {
            StringBuilder output = new StringBuilder();
            output.Append(this.Format(t.Subject));
            output.Append(this._deliminatorChar);
            output.Append(this.Format(t.Predicate));
            output.Append(this._deliminatorChar);
            output.Append(this.Format(t.Object));
            if (this._lineEndChar != null)
            {
                output.Append(this._lineEndChar);
            }
            return output.ToString();
        }

        protected override string FormatUriNode(UriNode u)
        {
            StringBuilder output = new StringBuilder();
            if (this._uriStartChar != null) output.Append(this._uriStartChar);
            if (this._uriEndChar != null)
            {
                output.Append(this.FormatUri(u.Uri));
                output.Append(this._uriEndChar);
            }
            else
            {
                output.Append(this.FormatUri(u.Uri));
            }
            return output.ToString();
        }

        protected override string FormatLiteralNode(LiteralNode lit)
        {
            StringBuilder output = new StringBuilder();
            if (TurtleSpecsHelper.IsValidPlainLiteral(lit.Value, lit.DataType))
            {
                output.Append(lit.Value);
            }
            else
            {
                String value = lit.Value;

                if (TurtleSpecsHelper.IsLongLiteral(value))
                {
                    value = value.Replace("\n", "\\n");
                    value = value.Replace("\r", "\\r");
                    value = value.Replace("\"", "\\\"");
                    value = value.Replace("\t", "\\t");

                    //If there are no wrapper characters then we must escape the deliminator
                    if (value.Contains(this._deliminatorChar))
                    {
                        if (this._literalWrapperChar == null && this._longLiteralWrapperChar == null)
                        {
                            //Replace the deliminator
                            value = value.Replace(new String(new char[] { this._deliminatorChar }), new String(new char[] { this._escapeChar, this._deliminatorChar }));
                        }
                    }

                    //Apply appropriate wrapper characters
                    if (this._longLiteralWrapperChar != null)
                    {
                        output.Append(this._longLiteralWrapperChar + value + this._longLiteralWrapperChar);
                    }
                    else if (this._literalWrapperChar != null)
                    {
                        output.Append(this._literalWrapperChar + value + this._literalWrapperChar);
                    }
                    else
                    {
                        output.Append(value);
                    }
                }
                else
                {
                    //Replace the deliminator
                    value = value.Replace(new String(new char[] { this._deliminatorChar }), new String(new char[] { this._escapeChar, this._deliminatorChar }));

                    //Apply appropriate wrapper characters
                    if (this._literalWrapperChar != null)
                    {
                        output.Append(this._literalWrapperChar + value + this._literalWrapperChar);
                    }
                    else
                    {
                        output.Append(value);
                    }
                }

                if (this._fullLiteralOutput)
                {
                    if (!lit.Language.Equals(String.Empty))
                    {
                        output.Append("@" + lit.Language);
                    }
                    else if (lit.DataType != null)
                    {
                        output.Append("^^");
                        if (this._uriStartChar != null) output.Append(this._uriStartChar);
                        if (this._uriEndChar != null)
                        {
                            output.Append(this.FormatUri(lit.DataType));
                            output.Append(this._uriEndChar);
                        }
                        else
                        {
                            output.Append(this.FormatUri(lit.DataType));
                        }
                    }
                }
            }
            return output.ToString();
        }

        public override string FormatUri(String u)
        {
            if (this._uriEndChar != null)
            {
                return u.Replace(new String(new char[] { (char)this._uriEndChar }), new String(new char[] { this._escapeChar, (char)this._uriEndChar }));
            }
            else
            {
                return u;
            }
        }
    }
}
