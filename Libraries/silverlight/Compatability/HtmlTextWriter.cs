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
using System.Text;
using System.IO;

namespace VDS.RDF
{
    public class HtmlTextWriter : TextWriter, IDisposable
    {
        public HtmlTextWriter(TextWriter writer)
        {

        }

        public override Encoding Encoding
        {
            get
            {
                   throw new NotImplementedException();
            }
        }

        public IFormatProvider FormatProvider
        {
            get
            {
                   throw new NotImplementedException();
            }
        }

        public Int32 Indent
        {
            get
            {
                   throw new NotImplementedException();
            }
            set
            {
                   throw new NotImplementedException();
            }
        }

        public TextWriter InnerWriter
        {
            get
            {
                   throw new NotImplementedException();
            }
            set
            {
                   throw new NotImplementedException();
            }
        }

        public String NewLine
        {
            get
            {
                   throw new NotImplementedException();
            }
            set
            {
                   throw new NotImplementedException();
            }
        }

        public void AddAttribute(String name, String value)
        {
            throw new NotImplementedException();
        }

        public void AddAttribute(String name, String value, Boolean fEndode)
        {
            throw new NotImplementedException();
        }

        public void AddAttribute(HtmlTextWriterAttribute key, String value)
        {
            throw new NotImplementedException();
        }

        public void AddAttribute(HtmlTextWriterAttribute key, String value, Boolean fEncode)
        {
            throw new NotImplementedException();
        }

        public void AddStyleAttribute(String name, String value)
        {
            throw new NotImplementedException();
        }

        public void AddStyleAttribute(HtmlTextWriterStyle key, String value)
        {
            throw new NotImplementedException();
        }

        public void BeginRender()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void EndRender()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public Boolean IsValidFormAttribute(String attribute)
        {
            throw new NotImplementedException();
        }

        public void RenderBeginTag(String tagName)
        {
            throw new NotImplementedException();
        }

        public void RenderBeginTag(HtmlTextWriterTag tagKey)
        {
            throw new NotImplementedException();
        }

        public void RenderEndTag()
        {
            throw new NotImplementedException();
        }

        public void Write(String s)
        {
            throw new NotImplementedException();
        }

        public void Write(Boolean value)
        {
            throw new NotImplementedException();
        }

        public void Write(Char value)
        {
            throw new NotImplementedException();
        }

        public void Write(Char[] buffer)
        {
            throw new NotImplementedException();
        }

        public void Write(Char[] buffer, Int32 index, Int32 count)
        {
            throw new NotImplementedException();
        }

        public void Write(Double value)
        {
            throw new NotImplementedException();
        }

        public void Write(Single value)
        {
            throw new NotImplementedException();
        }

        public void Write(Int32 value)
        {
            throw new NotImplementedException();
        }

        public void Write(Int64 value)
        {
            throw new NotImplementedException();
        }

        public void Write(Object value)
        {
            throw new NotImplementedException();
        }

        public void Write(String format, Object arg0)
        {
            throw new NotImplementedException();
        }

        public void Write(String format, Object arg0, Object arg1)
        {
            throw new NotImplementedException();
        }

        public void Write(String format, Object[] arg)
        {
            throw new NotImplementedException();
        }

        public void Write(UInt32 value)
        {
            throw new NotImplementedException();
        }

        public void Write(UInt64 value)
        {
            throw new NotImplementedException();
        }

        public void Write(Decimal value)
        {
            throw new NotImplementedException();
        }

        public void Write(String format, Object arg0, Object arg1, Object arg2)
        {
            throw new NotImplementedException();
        }

        public void WriteAttribute(String name, String value)
        {
            throw new NotImplementedException();
        }

        public void WriteAttribute(String name, String value, Boolean fEncode)
        {
            throw new NotImplementedException();
        }

        public void WriteBeginTag(String tagName)
        {
            throw new NotImplementedException();
        }

        public void WriteBreak()
        {
            throw new NotImplementedException();
        }

        public void WriteEncodedText(String text)
        {
            throw new NotImplementedException();
        }

        public void WriteEncodedUrl(String url)
        {
            throw new NotImplementedException();
        }

        public void WriteEncodedUrlParameter(String urlText)
        {
            throw new NotImplementedException();
        }

        public void WriteEndTag(String tagName)
        {
            throw new NotImplementedException();
        }

        public void WriteFullBeginTag(String tagName)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(String s)
        {
            throw new NotImplementedException();
        }

        public void WriteLine()
        {
            throw new NotImplementedException();
        }

        public void WriteLine(Boolean value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(Char value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(Char[] buffer)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(Char[] buffer, Int32 index, Int32 count)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(Double value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(Single value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(Int32 value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(Int64 value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(Object value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(String format, Object arg0)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(String format, Object arg0, Object arg1)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(String format, Object[] arg)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(UInt32 value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(UInt64 value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(Decimal value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(String format, Object arg0, Object arg1, Object arg2)
        {
            throw new NotImplementedException();
        }

        public void WriteLineNoTabs(String s)
        {
            throw new NotImplementedException();
        }

        public void WriteStyleAttribute(String name, String value)
        {
            throw new NotImplementedException();
        }

        public void WriteStyleAttribute(String name, String value, Boolean fEncode)
        {
            throw new NotImplementedException();
        }

    }
}

#endif