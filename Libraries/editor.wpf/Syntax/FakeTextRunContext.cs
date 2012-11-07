/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;

namespace VDS.RDF.Utilities.Editor.Wpf.Syntax
{
    public class FakeTextRunContext
        : ITextRunConstructionContext
    {
        public ICSharpCode.AvalonEdit.Document.TextDocument Document
        {
            get 
            { 
                return null; 
            }
        }

        public TextRunProperties GlobalTextRunProperties
        {
            get 
            { 
                return null; 
            }
        }

        public TextView TextView
        {
            get 
            { 
                return null;
            }
        }

        public VisualLine VisualLine
        {
            get 
            { 
                return null;
            }
        }

        public StringSegment GetText(int offset, int length)
        {
            return new StringSegment();
        }
    }
}
