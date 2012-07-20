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
        private String _mlStart, _mlEnd, _searchString, _fullLicense;
        private CheckerOptions _options;

        public CodeChecker(CheckerOptions options, String singleLineCommentStart, String multiLineCommentStart, String multiLineCommentEnd)
        {
            this._options = options;
            this._slMatch = new Regex(singleLineCommentStart + ".*" + options.SearchString, RegexOptions.IgnoreCase);
            this._mlStart = multiLineCommentStart;
            this._mlEnd = multiLineCommentEnd;
            this._searchString = options.SearchString;
            this._fullLicense = options.LicenseString;
        }

        public CodeChecker(CheckerOptions options)
            : this(options, DefaultSingleLineCommentStart, DefaultMultiLineCommentStart, DefaultMultiLineCommentEnd) { }

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

                if (text.Substring(startIndex, endIndex - startIndex).Contains(this._searchString))
                {
                    return true;
                }
                startIndex = endIndex;
            } while (true);
            return false;
        }

        protected override bool AddLicense(string file)
        {
            if (String.IsNullOrEmpty(this._fullLicense)) return false;
            try
            {
                String tempFile = Path.GetTempFileName();
                String existing = this.StripExistingHeader(File.ReadAllText(file));
                using (StreamWriter writer = new StreamWriter(tempFile))
                {
                    try
                    {
                        writer.WriteLine(this._mlStart);
                        writer.WriteLine(this._fullLicense);
                        writer.WriteLine(this._mlEnd);
                        writer.WriteLine();

                        //Don't do a WriteLine() on the existing data as we don't want to add unecessary new line to the end of the existing code
                        writer.Write(existing);
                        writer.Close();
                    }
                    catch
                    {
                        writer.Close();
                        return false;
                    }
                }

                //Copy the revised file over the old file
                File.Delete(file);
                File.Move(tempFile, file);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("LicenseChecker: Error: Failed to add license to file " + file + " - " + ex.Message);
                return false;
            }
        }

        protected String StripExistingHeader(String data)
        {
            if (data.StartsWith(this._mlStart))
            {
                if (!this._options.OverwriteExisting) throw new Exception("Not permitted to overwrite existing license headers");
                data = data.Substring(data.IndexOf(this._mlEnd) + this._mlEnd.Length);
                int start = 0;
                while (Char.IsWhiteSpace(data[start]) && start < data.Length)
                {
                    start++;
                }
                if (start > 0) data = data.Substring(start);
                return data;
            }
            else
            {
                return data;
            }
        }
    }
}
