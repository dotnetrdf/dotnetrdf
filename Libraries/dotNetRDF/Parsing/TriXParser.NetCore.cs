/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System.IO;
using System.Xml;

namespace VDS.RDF.Parsing
{
    public partial class TriXParser
    {
        private static XmlReaderSettings GetSettings()
        {
            return new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Document,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
            };
        }

        /// <inheritdoc />
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null)
            {
                throw new RdfParseException("Cannot parse an RDF Dataset using a null handler");
            }

            if (input == null)
            {
                throw new RdfParseException("Cannot parse an RDF Dataset from a null input");
            }


            try
            {
                // Get the reader and start parsing
                var reader = XmlReader.Create(input, GetSettings());
                RaiseWarning(
                    "The TriX Parser is operating without XSL support, if your TriX file requires XSL then it will not be parsed successfully");
                TryParseGraphset(reader, handler);

                input.Close();
            }
            catch (XmlException xmlEx)
            {
                // Wrap in a RDF Parse Exception
                throw new RdfParseException(
                    "Unable to Parse this TriX since the XmlReader encountered an error, see the inner exception for details",
                    xmlEx);
            }
            finally
            {
                input.Close();
            }
        }
    }
}