using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Common
{
    /// <summary>
    /// Interface for internal data structure of Hash Table
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface IHashSlot<T>
        : IEnumerable<T>, ICollection<T>
    {
    }
}
