using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Utilities.Convert.Inputs;

namespace VDS.RDF.Utilities.Convert
{
    static class ConversionExtensions
    {
        public static String GetFilename(this IConversionInput input, String baseName, String ext)
        {
            String outFile = String.Empty;
            if (!baseName.Equals(String.Empty))
            {
                outFile = baseName;
                if (input is FileInput)
                {
                    outFile += "_" + Path.GetFileNameWithoutExtension(((FileInput)input).SourceFile);
                }
                else if (input is UriInput)
                {
                    outFile += "_" + ((UriInput)input).SourceUri.GetSha256Hash();
                }
                outFile += ext;
                return outFile;
            }
            else
            {
                if (input is FileInput)
                {
                    outFile = Path.GetFileNameWithoutExtension(((FileInput)input).SourceFile);
                }
                else if (input is UriInput)
                {
                    outFile = ((UriInput)input).SourceUri.GetSha256Hash();
                }
                outFile += ext;
                return outFile;
            }
        }
    }
}
