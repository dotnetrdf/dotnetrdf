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
    public class LongTakeEnumerable<T>
        : WrapperEnumerable<T>
    {
        public LongTakeEnumerable(IEnumerable<T> enumerable, long toTake)
            : base(enumerable)
        {
            if (toTake <= 0) throw new ArgumentException("toTake must be > 0", "toTake");
            this.ToTake = toTake;
        }

        private long ToTake { get; set; }

        public override IEnumerator<T> GetEnumerator()
        {
            return new LongTakeEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.ToTake);
        }
    }

    public class LongTakeEnumerator<T>
        : WrapperEnumerator<T>
    {
        public LongTakeEnumerator(IEnumerator<T> enumerator, long toTake)
            : base(enumerator)
        {
            if (toTake <= 0) throw new ArgumentException("toTake must be > 0", "toTake");
            this.ToTake = toTake;
            this.Taken = 0;
        }

        private long ToTake { get; set; }

        private long Taken { get; set; }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);
            if (this.Taken >= this.ToTake)
            {
                return false;
            }
            if (!this.InnerEnumerator.MoveNext())
            {
                return false;
            }
            this.Taken++;
            item = this.InnerEnumerator.Current;
            return true;
        }

        protected override void ResetInternal()
        {
            this.Taken = 0;
        }
    }
}