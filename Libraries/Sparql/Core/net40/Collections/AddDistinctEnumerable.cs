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

namespace VDS.RDF.Collections
{
    public class AddDistinctEnumerable<T>
        : WrapperEnumerable<T>
    {
        public AddDistinctEnumerable(IEnumerable<T> enumerable, T item)
            : base(enumerable)
        {
            this.AdditionalItem = item;
        }

        private T AdditionalItem { get; set; }

        public override IEnumerator<T> GetEnumerator()
        {
            return new AddDistinctEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.AdditionalItem);
        }
    }

    public class AddDistinctEnumerator<T>
        : WrapperEnumerator<T>
    {

        public AddDistinctEnumerator(IEnumerator<T> enumerator, T item)
            : base(enumerator)
        {
            this.AdditionalItem = item;
            this.AdditionalItemSeen = false;
        }

        private bool AdditionalItemSeen { get; set; }

        private bool IsCurrentAdditionalItem { get; set; }

        private T AdditionalItem { get; set; }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);
            if (this.InnerEnumerator.MoveNext())
            {
                item = this.InnerEnumerator.Current;
                if (item.Equals(this.AdditionalItem)) this.AdditionalItemSeen = true;
                return true;
            }
            if (this.AdditionalItemSeen) return false;
            if (this.IsCurrentAdditionalItem) return false;

            item = this.AdditionalItem;
            this.IsCurrentAdditionalItem = true;
            return true;
        }

        protected override void ResetInternal()
        {
            this.AdditionalItemSeen = false;
            this.IsCurrentAdditionalItem = false;
        }
    }
}
