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
using VDS.RDF.Parsing.Validation;

namespace VDS.RDF.Utilities.Editor
{
    public class DocumentChangedEventArgs<T> : EventArgs
    {
        private Document<T> _doc;

        public DocumentChangedEventArgs(Document<T> doc)
        {
            this._doc = doc;
        }

        public Document<T> Document
        {
            get
            {
                return this._doc;
            }
        }
    }

    public class DocumentValidatedEventArgs<T> : DocumentChangedEventArgs<T>
    {
        private ISyntaxValidationResults _results;

        public DocumentValidatedEventArgs(Document<T> doc, ISyntaxValidationResults results)
            : base(doc)
        {
            this._results = results;
        }

        public ISyntaxValidationResults ValidationResults
        {
            get
            {
                return this._results;
            }
        }
    }

    public delegate void DocumentChangedHandler<T>(Object sender, DocumentChangedEventArgs<T> args);

    public delegate void DocumentValidatedHandler<T>(Object sender, DocumentValidatedEventArgs<T> args);

    public delegate bool DocumentCallback<T>(Document<T> doc);

    public enum SaveChangesMode
    {
        Save,
        Discard,
        Cancel
    }

    public delegate SaveChangesMode SaveChangesCallback<T>(Document<T> doc);

    public delegate String SaveAsCallback<T>(Document<T> doc);
}
