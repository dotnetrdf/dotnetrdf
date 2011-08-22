using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Standard Database schemas provided by the library
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ADO Store was specifically designed to be completely based upon use of stored procedures so any schema can potentially be used provided it exposed the stored procedures that the code expects to exist.
    /// </para>
    /// <para>
    /// There are two schemas provided by default, the <see cref="AdoSchema.Hash">Hash</see> schema is recommended but requires SQL Server 2005 or higher.  The <see cref="AdoSchema.Simple">Simple</see> schema should typically only be used where you need to use an earlier version of SQL Server e.g. SQL Server 2000.
    /// </para>
    /// </remarks>
    public enum AdoSchema
    {
        /// <summary>
        /// A simple schema that uses partial value indexes to speed up Node ID lookups
        /// </summary>
        /// <remarks>
        /// <strong>Note:</strong> Uses features only available in SQL Server 2005 and later versions
        /// </remarks>
        Simple,
        /// <summary>
        /// A more advanced schema that uses MD5 hash based indexes to provide better Node ID lookup speed
        /// </summary>
        /// <remarks>
        /// <strong>Note:</strong> Uses features only available in SQL Server 2005 and later versions
        /// </remarks>
        Hash
    }
}
