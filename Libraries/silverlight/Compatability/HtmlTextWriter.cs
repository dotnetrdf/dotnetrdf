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

#if NO_WEB

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VDS.RDF.Writing;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Custom implementation of <see cref="System.Web.UI.HtmlTextWriter">System.Web.UI.HtmlTextWriter</see> to replace it in builds where System.Web is not available
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this is not a full implementation of HtmlTextWriter as per the original class, it simply emulates all the functionality that dotNetRDF requires for it's HTML outputting
    /// </para>
    /// </remarks>
    public class HtmlTextWriter : TextWriter, IDisposable
    {
        private TextWriter _writer;
        private bool _newline = true;
        private int _indent = 0;
        private Stack<String> _tags = new Stack<String>();
        private List<KeyValuePair<String, String>> _attributes = new List<KeyValuePair<String, String>>();
        private List<KeyValuePair<String, String>> _styles = new List<KeyValuePair<String, String>>();

        /// <summary>
        /// Creates a new HTML Text Writer
        /// </summary>
        /// <param name="writer">Text Writer</param>
        public HtmlTextWriter(TextWriter writer)
        {
            this._writer = writer;
        }

        /// <summary>
        /// Gets the encoding of the Inner Writer
        /// </summary>
        public override Encoding Encoding
        {
            get
            {
                return this._writer.Encoding;
            }
        }

        /// <summary>
        /// Gets/Sets the current Indent
        /// </summary>
        public Int32 Indent
        {
            get
            {
                return this._indent;
            }
            set
            {
                this._indent = value;
            }
        }

        /// <summary>
        /// Gets the Inner Writer
        /// </summary>
        public TextWriter InnerWriter
        {
            get
            {
                return this._writer;
            }
        }

        private String EncodeAttribute(String value)
        {
            value = WriterHelper.EncodeForXml(value);
            if (value.EndsWith("&")) value += "amp;";
            value = value.Replace("\"", "&quot;");
            value = value.Replace("<", "&lt;");
            value = value.Replace(">", "&gt;");
            return value;
        }

        private String EncodeStyle(String value)
        {
            value = WriterHelper.EncodeForXml(value);
            if (value.EndsWith("&")) value += "amp;";
            value = value.Replace("\"", "'");
            value = value.Replace("<", "&lt;");
            value = value.Replace(">", "&gt;");
            return value;
        }

        private String GetAttributeName(HtmlTextWriterAttribute key)
        {
            String name = key.ToString().ToLower();
            if (name.Contains(".")) name = name.Substring(name.LastIndexOf(".") + 1);
            return name;
        }

        private String GetStyleName(HtmlTextWriterStyle key)
        {
            String name = key.ToString();
            if (name.Contains(".")) name = name.Substring(name.LastIndexOf(".") + 1);

            StringBuilder output = new StringBuilder();
            char[] cs = name.ToCharArray();
            for (int i = 0; i < cs.Length; i++)
            {
                if (Char.IsUpper(cs[i]) && i > 0)
                {
                    output.Append('-');
                    output.Append(cs[i]);
                }
                else
                {
                    output.Append(cs[i]);
                }
            }
            return output.ToString();
        }

        private String GetTagName(HtmlTextWriterTag key)
        {
            String name = key.ToString().ToLower();
            if (name.Contains(".")) name = name.Substring(name.LastIndexOf(".") + 1);
            return name;
        }

        public void AddAttribute(String name, String value)
        {
            this._attributes.Add(new KeyValuePair<String, String>(name, this.EncodeAttribute(value)));
        }

        public void AddAttribute(String name, String value, Boolean fEncode)
        {
            if (fEncode) value = this.EncodeAttribute(value);
            this._attributes.Add(new KeyValuePair<String, String>(name, value));
        }

        public void AddAttribute(HtmlTextWriterAttribute key, String value)
        {
            this.AddAttribute(this.GetAttributeName(key), value);
        }

        public void AddAttribute(HtmlTextWriterAttribute key, String value, Boolean fEncode)
        {
            this.AddAttribute(this.GetAttributeName(key), value, fEncode);
        }

        public void AddStyleAttribute(String name, String value)
        {
            this._styles.Add(new KeyValuePair<String, String>(name, value));
        }

        public void AddStyleAttribute(HtmlTextWriterStyle key, String value)
        {
            this._styles.Add(new KeyValuePair<String, String>(this.GetStyleName(key), value));
        }

        public void Close()
        {
            this._writer.Close();
        }

        public void Flush()
        {
            this._writer.Flush();
        }

        public void RenderBeginTag(String tagName)
        {
            this._tags.Push(tagName);
            if (this._newline)
            {
                this._writer.Write(new String('\t', this._indent));
                this._newline = false;
            }
            this._writer.Write("<" + tagName.ToLower());
            foreach (KeyValuePair<String, String> attr in this._attributes)
            {
                this._writer.Write(" " + attr.Key + "=\"" + attr.Value + "\"");
            }
            this._attributes.Clear();

            if (this._styles.Count > 0)
            {
                this._writer.Write(" style=\"");
                foreach (KeyValuePair<String, String> style in this._styles)
                {
                    this._writer.Write(style.Key + ": " + this.EncodeStyle(style.Value) + ";");
                }
                this._writer.Write("\"");
                this._styles.Clear();
            }
            this._writer.WriteLine(">");
            this._newline = true;
            this._indent++;
        }

        public void RenderBeginTag(HtmlTextWriterTag tagKey)
        {
            this.RenderBeginTag(this.GetTagName(tagKey));
        }

        public void RenderEndTag()
        {
            if (this._tags.Count > 0)
            {
                this._indent--;
                if (!this._newline) this._writer.WriteLine();
                this._writer.Write(new String('\t', this._indent));
                this._writer.WriteLine("</" + this._tags.Pop().ToLower() + ">");
                this._newline = true;
            }
            else
            {
                throw new Exception("Cannot end a tag as there are no open tags");
            }
        }

        public override void Write(char value)
        {
            this._writer.Write(value);
        }

        public override void Write(bool value)
        {
            this._writer.Write(value);
        }

        public override void Write(char[] buffer)
        {
            this._writer.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            this._writer.Write(buffer, index, count);
        }

        public override void Write(decimal value)
        {
            this._writer.Write(value);
        }

        public override void Write(double value)
        {
            this._writer.Write(value);
        }

        public override void Write(float value)
        {
            this._writer.Write(value);
        }

        public override void Write(int value)
        {
            this._writer.Write(value);
        }

        public override void Write(long value)
        {
            this._writer.Write(value);
        }

        public override void Write(object value)
        {
            this._writer.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            this._writer.Write(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            this._writer.Write(format, arg0, arg1);
        }

        public override void Write(string format, params object[] arg)
        {
            this._writer.Write(format, arg);
        }

        public override void Write(string value)
        {
            this._writer.Write(value);
        }

        public override void Write(uint value)
        {
            this._writer.Write(value);
        }

        public override void Write(ulong value)
        {
            this._writer.Write(value);
        }

        public void WriteAttribute(String name, String value)
        {
            this._writer.Write(name + "=\"" + this.EncodeAttribute(value) + "\"");
        }

        public void WriteAttribute(String name, String value, Boolean fEncode)
        {
            if (fEncode) value = this.EncodeAttribute(value);
            this._writer.Write(name + "=\"" + value + "\"");
        }

        public void WriteBeginTag(String tagName)
        {
            if (this._newline)
            {
                this._writer.Write(new String('\t', this._indent));
                this._newline = false;
            }
            this._writer.Write("<" + tagName.ToLower());
        }

        public void WriteBreak()
        {
            if (this._newline)
            {
                this._writer.WriteLine(new String('\t', this._indent));
                this._newline = false;
            }
            this._writer.Write("<br />");
        }

        public void WriteEncodedText(String text)
        {
            if (this._newline)
            {
                this._writer.Write(new String('\t', this._indent));
                this._newline = false;
            }
            this._writer.Write(text);
        }

        public void WriteEncodedUrl(String url)
        {
            this._writer.Write(Uri.EscapeUriString(url));
        }

        public void WriteEncodedUrlParameter(String urlText)
        {
            this._writer.Write(Uri.EscapeDataString(urlText));
        }

        public void WriteEndTag(String tagName)
        {
            if (this._newline)
            {
                this._writer.Write(new String('\t', this._indent));
                this._newline = false;
            }
            this._writer.Write("</" + tagName.ToLower() + ">");
        }

        public void WriteFullBeginTag(String tagName)
        {
            if (this._newline)
            {
                this._writer.Write(new String('\t', this._indent));
                this._newline = false;
            }
            this._writer.Write("<" + tagName.ToLower() + ">");
        }

        public override void WriteLine()
        {
            this._writer.WriteLine();
        }

        public override void WriteLine(bool value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(char value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(char[] buffer)
        {
            this._writer.WriteLine(buffer);
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            this._writer.WriteLine(buffer, index, count);
        }

        public override void WriteLine(decimal value)
        {
             this._writer.WriteLine(value);
        }

        public override void WriteLine(double value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(float value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(int value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(long value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(object value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(string value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(string format, object arg0)
        {
            this._writer.WriteLine(format, arg0);
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            this._writer.WriteLine(format, arg0, arg1);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            this._writer.WriteLine(format, arg);
        }

        public override void WriteLine(uint value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(ulong value)
        {
            this._writer.WriteLine(value);
        }

        public void WriteLineNoTabs(String s)
        {
            throw new NotImplementedException();
        }

        public void WriteStyleAttribute(String name, String value)
        {
            this._writer.Write(name + ": " + this.EncodeStyle(value) + ";");
        }

        public void WriteStyleAttribute(String name, String value, Boolean fEncode)
        {
            if (fEncode) value = this.EncodeAttribute(value);
            this._writer.Write(name + ": " + value + ";");
        }

    }
}

#endif