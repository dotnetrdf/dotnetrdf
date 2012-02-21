using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LicenseChecker.Checkers
{
    interface ILicenseChecker
    {
        bool IsLicensed(String file);
    }
}
