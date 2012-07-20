using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LicenseChecker.Checkers
{
    class XmlChecker
        : BaseChecker
    {
        private String _licenseString;

        public XmlChecker(String licenseString)
        {
            this._licenseString = licenseString;
        }

        protected override bool CheckLicense(string file)
        {
            if (File.Exists(file))
            {
                using (XmlTextReader reader = new XmlTextReader(file))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Comment)
                        {
                            if (reader.ReadInnerXml().Contains(this._licenseString)) return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        protected override bool AddLicense(string file)
        {
            return false;
        }
    }
}
