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

using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public class ReducedEnumerable<T>
        : WrapperEnumerable<T>
    {
        public ReducedEnumerable(IEnumerable<T> enumerable)
            : base(enumerable) { }

        public override IEnumerator<T> GetEnumerator()
        {
            return new ReducedEnumerator<T>(this.InnerEnumerable.GetEnumerator());
        }
    }

    public class ReducedEnumerator<T>
        : WrapperEnumerator<T>
    {
        public ReducedEnumerator(IEnumerator<T> enumerator)
            : base(enumerator)
        {
            this.First = true;
        }

        private bool First { get; set; }

        private T LastItem { get; set; }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);
            if (this.InnerEnumerator.MoveNext()) return false;
            item = this.InnerEnumerator.Current;

            if (this.First)
            {
                this.First = false;
                this.LastItem = item;
                return true;
            }

            while (true)
            {
                // Provided the next item is not the same as the previous return it
                if (!this.LastItem.Equals(item))
                {
                    this.LastItem = item;
                    return true;
                }

                if (!this.InnerEnumerator.MoveNext()) return false;
                item = this.InnerEnumerator.Current;
            }
        }

        protected override void ResetInternal()
        {
            this.First = true;
            this.LastItem = default(T);
        }
    }
}
