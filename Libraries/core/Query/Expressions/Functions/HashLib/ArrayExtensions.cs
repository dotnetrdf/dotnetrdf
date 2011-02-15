using System;
using System.Diagnostics;

namespace HashLib
{
    /// <summary>
    /// Code incorporated from the <a href="http://hashlib.codeplex.com">HashLib</a> project to support SHA224 and provide support for all Hash functions under Silverlight
    /// </summary>
    /// <remarks>
    /// Slightly modified to downgrade the code to C# 3 syntax
    /// </remarks>
    [DebuggerStepThrough]
    public static class ArrayExtensions
    {
        public static void Clear(this Array a_array)
        {
            Array.Clear(a_array, 0, a_array.Length);
        }
    }
}
