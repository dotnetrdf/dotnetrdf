using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LicenseChecker.Checkers
{
    abstract class BaseChecker
        : ILicenseChecker
    {
        public bool IsLicensed(string file)
        {
            if (File.Exists(file))
            {
                return this.CheckLicense(file);
            }
            else
            {
                return false;
            }
        }

        protected abstract bool CheckLicense(String file);

        public bool FixLicense(String file)
        {
            if (File.Exists(file))
            {
                return this.AddLicense(file);
            }
            else
            {
                return false;
            }
        }

        protected abstract bool AddLicense(String file);
    }
}
