using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LicenseChecker.Checkers
{
    static class CheckerFactory
    {
        private static ILicenseChecker _cStyleChecker;
        private static ILicenseChecker _vbStyleChecker;
        private static ILicenseChecker _xmlChecker;

        public static void Init(CheckerOptions options)
        {
            _cStyleChecker = new CodeChecker(options);
            _vbStyleChecker = new CodeChecker(options, "'", null, null);
            _xmlChecker = new XmlChecker(options.SearchString);
        }

        public static ILicenseChecker GetChecker(String file)
        {
            switch (Path.GetExtension(file))
            {
                case ".cs":
                case ".java":
                case ".cpp":
                    return _cStyleChecker;
                case ".vb":
                    return _vbStyleChecker;
                case ".xml":
                    return _xmlChecker;
                default:
                    return null;
            }
        }
    }
}
