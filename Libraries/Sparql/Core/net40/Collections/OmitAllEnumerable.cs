/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Collections
{
    public class OmitAllEnumerable<T>
        : WrapperEnumerable<T>
    {
        public OmitAllEnumerable(IEnumerable<T> enumerable, T item)
            : base(enumerable)
        {
            this.OmittedItem = item;
        }

        private T OmittedItem { get; set; }

        public override IEnumerator<T> GetEnumerator()
        {
            return new OmitAllEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.OmittedItem);
        }
    }

    public class OmitAllEnumerator<T>
        : WrapperEnumerator<T>
    {
        public OmitAllEnumerator(IEnumerator<T> enumerator, T item)
            : base(enumerator)
        {
            this.OmittedItem = item;
        }

        private T OmittedItem { get; set; }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);
            while (this.InnerEnumerator.MoveNext())
            {
                item = this.InnerEnumerator.Current;
                if (item.Equals(this.OmittedItem)) continue;

                return true;
            }
            return false;
        }
    }
}
