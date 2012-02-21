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

        public static void Init(String licenseString)
        {
            _cStyleChecker = new CodeChecker(licenseString);
            _vbStyleChecker = new CodeChecker(licenseString, "'", null, null);
            _xmlChecker = new XmlChecker(licenseString);
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
