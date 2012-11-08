/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
