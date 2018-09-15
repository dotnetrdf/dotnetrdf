namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public partial class DynamicGraph
    {
        public ICollection<object> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => false;

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}
