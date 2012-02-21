using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LicenseChecker.Checkers
{
    class CodeChecker
        : BaseChecker
    {
        private const String DefaultSingleLineCommentStart = "//",
                             DefaultMultiLineCommentStart = "/*",
                             DefaultMultiLineCommentEnd = "*/";

        private Regex _slMatch;
        private String _mlStart, _mlEnd, _licenseString;

        public CodeChecker(String licenseString, String singleLineCommentStart, String multiLineCommentStart, String multiLineCommentEnd)
        {
            this._slMatch = new Regex(singleLineCommentStart + ".*" + licenseString, RegexOptions.IgnoreCase);
            this._mlStart = multiLineCommentStart;
            this._mlEnd = multiLineCommentEnd;
            this._licenseString = licenseString;
        }

        public CodeChecker(String licenseString)
            : this(licenseString, DefaultSingleLineCommentStart, DefaultMultiLineCommentStart, DefaultMultiLineCommentEnd) { }

        protected override bool CheckLicense(string file)
        {
            String text = File.ReadAllText(file);
            return this._slMatch.IsMatch(text) || this.HasMultiLineMatch(text);
        }

        private bool HasMultiLineMatch(String text)
        {
            int startIndex = 0, endIndex = -1;
            do
            {
                startIndex = text.IndexOf(this._mlStart, startIndex);
                if (startIndex == -1) break;

                endIndex = text.IndexOf(this._mlEnd, startIndex);
                if (endIndex == -1) break;

                if (text.Substring(startIndex, endIndex - startIndex).Contains(this._licenseString))
                {
                    return true;
                }
                startIndex = endIndex;
            } while (true);
            return false;
        }
    }
}
