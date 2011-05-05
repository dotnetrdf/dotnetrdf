/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Utilities.Convert.Inputs;

namespace VDS.RDF.Utilities.Convert
{
    /// <summary>
    /// Does RDF Conversions using the dotNetRDF command line syntax
    /// </summary>
    public class RdfConvert
    {
        //Option Variables
        private List<IConversionInput> _inputs = new List<IConversionInput>();
        private List<IConversionOption> _options = new List<IConversionOption>();
        private bool _overwrite = false;
        private bool _debug = false;
        private bool _warnings = false;
        private bool _best = false;
 
        //Output Variables
        private String _outputFilename = String.Empty;
        private String _outFormat = null;
        private String _outExt = null;

        public void RunConvert(String[] args)
        {
            //Set the Options
            if (!this.SetOptions(args))
            {
                //If SetOptions returns false then some options were invalid and errors have been output to the error stream
                return;
            }


            Console.WriteLine("rdfConvert: Not Yet Implemented");
        }

        private bool SetOptions(String[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && args[0].Equals("-help")))
            {
                this.ShowUsage();
                return false;
            }

            //Look through the arguments to see what we've been asked to do
            foreach (String arg in args)
            {
                if (arg.StartsWith("-hs"))
                {
                    if (arg.Contains(':'))
                    {
                        bool hs;
                        if (Boolean.TryParse(arg.Substring(arg.IndexOf(':') + 1), out hs))
                        {
                            this._options.Add(new HighSpeedOption(hs));
                        }
                        else
                        {
                            this._options.Add(new HighSpeedOption(true));
                        }
                    }
                    else
                    {
                        this._options.Add(new HighSpeedOption(true));
                    }
                }
                else if (arg.StartsWith("-pp"))
                {
                    if (arg.Contains(':'))
                    {
                        bool pp;
                        if (Boolean.TryParse(arg.Substring(arg.IndexOf(':') + 1), out pp))
                        {
                            this._options.Add(new PrettyPrintingOption(pp));
                        }
                        else
                        {
                            this._options.Add(new PrettyPrintingOption(true));
                        }
                    }
                    else
                    {
                      this._options.Add(new PrettyPrintingOption(true));
                    }
                }
                else if (arg.StartsWith("-c"))
                {
                    if (arg.Contains(':'))
                    {
                        int c;
                        if (Int32.TryParse(arg.Substring(arg.IndexOf(':') + 1), out c))
                        {
                            this._options.Add(new CompressionLevelOption(c));
                        }
                        else
                        {
                            this._options.Add(new CompressionLevelOption(WriterCompressionLevel.Default));
                        }
                    }
                    else
                    {
                        this._options.Add(new CompressionLevelOption(WriterCompressionLevel.Default));
                    }
                }
                else if (arg.StartsWith("-stylesheet:"))
                {
                    String stylesheet = arg.Substring(arg.IndexOf(':') + 1);
                    this._options.Add(new StylesheetOption(stylesheet));

                }
                else if (arg.Equals("-overwrite"))
                {
                    this._overwrite = true;
                }
                else if (arg.StartsWith("-out:") || arg.StartsWith("-output:"))
                {
                    this._outputFilename = arg.Substring(arg.IndexOf(':') + 1);
                }
                else if (arg.StartsWith("-format:"))
                {
                    String format = arg.Substring(arg.IndexOf(':') + 1);
                    if (!format.Contains("/"))
                    {
                        try
                        {
                            format = MimeTypesHelper.GetMimeType(format);
                        }
                        catch
                        {
                            Console.Error.WriteLine("rdfConvert: Error: The File Extension '" + format + "' is not permissible since dotNetRDF cannot infer a MIME type from the extension");
                            return false;
                        }
                    }
                    //Validate the MIME Type
                    if (!IsValidMimeType(format))
                    {
                        Console.Error.WriteLine("rdfConvert: Error: The MIME Type '" + format + "' is not permissible since dotNetRDF does not support outputting in that format");
                        return false;
                    }
                    this._outFormat = format;
                }
                else if (arg.StartsWith("-outext:"))
                {
                    this._outExt = arg.Substring(arg.IndexOf(':') + 1);
                    if (!this._outExt.StartsWith(".")) this._outExt = "." + this._outExt;
                }
                else if (arg.Equals("-debug"))
                {
                    this._debug = true;
                }
                else if (arg.Equals("-help"))
                {
                    //Ignore help argument if other arguments are present
                }
                else if (arg.Equals("-nocache"))
                {
                    Options.UriLoaderCaching = false;
                }
                else if (arg.Equals("-nobom"))
                {
                    Options.UseBomForUtf8 = false;
                }
                else if (arg.Equals("-warnings"))
                {
                    this._warnings = true;
                    UriLoader.Warning += this.ShowWarning;
                    UriLoader.StoreWarning += this.ShowWarning;
                    FileLoader.Warning += this.ShowWarning;
                    FileLoader.StoreWarning += this.ShowWarning;
                }
                else
                {
                    //Anything else is treated as an input
                    if (arg.Contains("://"))
                    {
                        try
                        {
                            this._inputs.Add(new UriInput(new Uri(arg)));
                        }
                        catch (UriFormatException uriEx)
                        {
                            Console.Error.WriteLine("rdfConvert: Error: The Input '" + arg + "' which rdfConvert assumed to be a URI is not a valid URI - " + uriEx.Message);
                            return false;
                        }
                    }
                    else
                    {
                        this._inputs.Add(new FileInput(arg));
                    }
                }
            }

            //If there are no this._inputs then we'll abort
            if (this._inputs.Count == 0)
            {
                Console.Error.WriteLine("rdfConvert: Abort: No Inputs were provided - please provide one/more files or URIs you wish to convert");
                return false;
            }

            //If there are no writers specified then we'll abort
            if (this._outFormat == null)
            {
                Console.Error.WriteLine("rdfConvert: Abort: Aborting since no output options have been specified, use the -out:filename or -outformat: arguments to specify output format");
                return false;
            }

            //Ensure Output Extension (if specified) is OK
            if (!this._outExt.Equals(String.Empty))
            {
                if (!this._outExt.StartsWith(".")) this._outExt = "." + this._outExt;
            }

            //If more than one input and an Output Filename be sure to strip any Extension from the Filename
            if (this._inputs.Count > 1 && !this._outputFilename.Equals(String.Empty))
            {
                this._outputFilename = Path.GetFileNameWithoutExtension(this._outputFilename);
            }

            return true;
        }

        private void ShowUsage()
        {
            Console.WriteLine("rdfConvert Utility for dotNetRDF");
            Console.WriteLine("--------------------------------");
            Console.WriteLine();
            Console.WriteLine("Command usage is as follows:");
            Console.WriteLine("rdfConvert input1 [input2 [input3 [...]]] (-out:filename.ext | -outformat:mime/type) [options]");
            Console.WriteLine();
            Console.WriteLine("e.g. rdfConvert input.rdf -out:output.ttl");
            Console.WriteLine("e.g. rdfConvert input1.rdf input2.ttl input3.n3 -format:text/html");
            Console.WriteLine("e.g. rdfConvert input.rdf -ext:n3");
            Console.WriteLine();
            Console.WriteLine("You can use URIs as input just by stating URIs (anything with a :// in it will be assumed to be a URI) e.g.");
            Console.WriteLine("rdfConvert http://example.org/something -out:something.rdf");
            Console.WriteLine();
            Console.WriteLine("Notes");
            Console.WriteLine("-----");
            Console.WriteLine();
            Console.WriteLine("rdfConvert may be used to convert between Dataset (NQuads, TriG etc) formats as well as Graph formats");
            Console.WriteLine();
            Console.WriteLine("Supported Options");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine("-best");
            Console.WriteLine("Causes the utility to attempt the best conversion it can (i.e. most compressed syntax) taking into account other options like compression level.  May cause conversions to be slower and require more memory");
            Console.WriteLine();
            Console.WriteLine("-c[:integer]");
            Console.WriteLine("Sets the Compression Level used by compressing writers, if specified without an integer parameter then defaults to default compression");
            Console.WriteLine();
            Console.WriteLine("-debug");
            Console.WriteLine("Prints more detailed error messages if errors occur");
            Console.WriteLine();
            Console.WriteLine("-ext:ext");
            Console.WriteLine("Overrides the default file extension which will be automatically determined based on the -out/-format option.  Must occur after the -out/-format option or it may be ignored");
            Console.WriteLine();
            Console.WriteLine("-format:(mime/type|ext)");
            Console.WriteLine("Specifies an output format in terms of a MIME Type or a file extension, if the MIME type/file extension does not correspond to a supported RDF Graph/Dataset format then the utility aborts.");
            Console.WriteLine();
            Console.WriteLine("-help");
            Console.WriteLine("Prints this usage summary if it is the only argument, otherwise ignored");
            Console.WriteLine();
            Console.WriteLine("-hs[:(true|false)]");
            Console.WriteLine("Enables/Disables High Speed write mode, if specified without a boolean parameter then defaults to enabled");
            Console.WriteLine();
            Console.WriteLine("-nobom");
            Console.WriteLine("Specifies that no BOM should be used for UTF-8 Output");
            Console.WriteLine();
            Console.WriteLine("-nocache");
            Console.WriteLine("Specifies that caching of input URIs is disabled i.e. forces the RDF to be retrieved directly from the URI bypassing any locally cached copy");
            Console.WriteLine();
            Console.WriteLine("-out:filename.ext");
            Console.WriteLine("-output:filename.ext");
            Console.WriteLine("Specifies a specific file to output to (assuming only 1 input), if more than one input is specified then this parameter sets the base filename for outputs (extension ignored in this case)");
            Console.WriteLine();
            Console.WriteLine("-overwrite");
            Console.WriteLine("Specifies that the utility can overwrite existing files");
            Console.WriteLine();
            Console.WriteLine("-pp[:(true|false)]");
            Console.WriteLine("Enables/Disables Pretty Printing, if specified without a boolean parameter then defaults to enabled");
            Console.WriteLine();
            Console.WriteLine("-rapper");
            Console.WriteLine("Runs rdfConvert in rapper compatibility mode, type rdfConvert -rapper -h for further information.  Must be the first argument or ignored");
            Console.WriteLine();
            Console.WriteLine("-warnings");
            Console.WriteLine("Shows Warning Messages output by Parsers and Serializers");
        }

        public static bool IsValidMimeType(String mimeType)
        {
            if (MimeTypesHelper.GetDefinitions(mimeType).Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void DebugErrors(Exception ex)
        {
            Console.Error.WriteLine("rdfConvert: Error Stack Trace:");
            Console.Error.WriteLine(ex.StackTrace);

            while (ex.InnerException != null)
            {
                Console.Error.WriteLine("rdfConvert: Error: " + ex.InnerException.Message);
                Console.Error.WriteLine("rdfConvert: Error Stack Trace:");
                Console.Error.WriteLine(ex.InnerException.StackTrace);
                ex = ex.InnerException;
            }
        }

        private void ShowWarning(String message)
        {
            Console.Error.WriteLine("rdfConvert: Warning: " + message);
        }
    }
}
