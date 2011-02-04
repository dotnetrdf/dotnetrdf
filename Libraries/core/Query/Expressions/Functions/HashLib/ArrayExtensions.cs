using System;
using System.Diagnostics;

namespace HashLib
{
    [DebuggerStepThrough]
    public static class ArrayExtensions
    {
        public static void Clear(this Array a_array)
        {
            Array.Clear(a_array, 0, a_array.Length);
        }
    }
}
