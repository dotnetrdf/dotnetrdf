using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LicenseChecker.Providers
{
    /// <summary>
    /// Interface for providers which are used to determine the source files for checking
    /// </summary>
    interface ISourceProvider
    {
        /// <summary>
        /// Gets all eligible source files
        /// </summary>
        /// <returns></returns>
        IEnumerable<String> GetSourceFiles();
    }
}
