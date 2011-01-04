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

namespace rdfConvert
{
    /// <summary>
    /// Does RDF Conversions using the dotNetRDF command line syntax
    /// </summary>
    public class RdfConvert
    {
        //Option Variables
        private List<String> _inputs = new List<string>();
        private String _output = String.Empty;
        private List<IConversionOption> _options = new List<IConversionOption>();
        private bool _merge = false;
        private bool _overwrite = false;
        private bool _dataset = false;
        private bool _debug = false;
        private bool _warnings = false;
 
        //Writers we'll configure as necessary
        private IRdfWriter _writer = null;
        private IStoreWriter _storeWriter = null;
        private String _outExt = String.Empty;

        //Temporary Store where we'll put the Graphs we've been asked to convert if necessary
        //We may not need this
        private TripleStore _store = new TripleStore();

        public void RunConvert(String[] args)
        {
            //Set the Options
            if (!this.SetOptions(args))
            {
                //If SetOptions returns false then some options were invalid and errors have been output to the error stream
                return;
            }

            //Then we'll read in our inputs
            foreach (String input in this._inputs)
            {
                try
                {
                    Graph g = new Graph();
                    if (input.StartsWith("-uri:"))
                    {
                        UriLoader.Load(g, new Uri(input.Substring(input.IndexOf(':') + 1)));
                    }
                    else
                    {
                        FileLoader.Load(g, input);
                    }

                    //If not merging we'll output now
                    if (!this._merge)
                    {
                        String destFile;
                        if (input.StartsWith("-uri:"))
                        {
                            if (this._inputs.Count == 1)
                            {
                                //For a single URI input we require a Filename
                                if (this._output.Equals(String.Empty))
                                {
                                    Console.Error.WriteLine("rdfConvert: When converting a single URI you must specify an output file with the -out:filename argument");
                                    return;
                                }
                                destFile = Path.GetFileNameWithoutExtension(this._output) + this._outExt;
                            }
                            else
                            {
                                //For multiple inputs where some are URIs the output file is the SHA256 hash of the URI plus the extension
                                destFile = new Uri(input.Substring(input.IndexOf(':') + 1)).GetSha256Hash() + this._outExt;
                            }
                        }
                        else
                        {
                            if (this._inputs.Count == 1 && !this._output.Equals(String.Empty))
                            {
                                //For a single input we'll just change the extension as appropriate
                                if (!this._outExt.Equals(String.Empty))
                                {
                                    destFile = Path.GetFileNameWithoutExtension(this._output) + this._outExt;
                                }
                                else
                                {
                                    destFile = this._output;
                                }
                            }
                            else
                            {
                                destFile = Path.GetFileNameWithoutExtension(input) + this._outExt;
                            }
                        }
                        if (File.Exists(destFile) && !this._overwrite)
                        {
                            Console.Error.WriteLine("rdfConvert: Unable to output to '" + destFile + "' because a file already exists at this location and the -overwrite argument was not used");
                        }
                        else
                        {
                            try
                            {
                                this._writer.Save(g, destFile);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine("rdfConvert: Unable to output to '" + destFile + "' due to the following error:"); 
                                Console.Error.WriteLine("rdfConvert: Error: " + ex.Message);
                                if (this._debug) this.DebugErrors(ex);
                            }
                        }
                    }
                    else
                    {
                        //Add to the Store and we'll merge it all together later and output it at the end
                        this._store.Add(g);
                    }
                }
                catch (RdfParserSelectionException parseEx)
                {
                    //If this happens then this may be a datset instead of a graph
                    try
                    {
                        if (input.StartsWith("-uri:"))
                        {
                            UriLoader.Load(this._store, new Uri(input.Substring(input.IndexOf(':') + 1)));
                        }
                        else
                        {
                            FileLoader.Load(this._store, input);
                        }

                        //If not merging we'll output now
                        if (!this._merge)
                        {
                            foreach (IGraph g in this._store.Graphs)
                            {
                                String destFile = (g.BaseUri == null) ? "default-graph" : g.BaseUri.GetSha256Hash();
                                destFile += this._outExt;

                                if (File.Exists(destFile))
                                {
                                    Console.Error.WriteLine("rdfConvert: Unable to output to '" + destFile + "' because a file already exists at this location and the -overwrite argument was not used");
                                }
                                else
                                {
                                    try
                                    {
                                        this._writer.Save(g, destFile);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.Error.WriteLine("rdfConvert: Unable to output to '" + destFile + "' due to the following error:");
                                        Console.Error.WriteLine("rdfConvert: Error: " + ex.Message);
                                        if (this._debug) this.DebugErrors(ex);
                                    }
                                }
                            }

                            //Reset the Triple Store after outputting
                            this._store.Dispose();
                            this._store = new TripleStore();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("rdfConvert: Unable to read from input '" + input + "' due to the following error:");
                        Console.Error.WriteLine("rdfConvert: Error: " + ex.Message);
                        if (this._debug) this.DebugErrors(ex);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("rdfConvert: Unable to read from input '" + input + "' due to the following error:");
                    Console.Error.WriteLine("rdfConvert: Error: " + ex.Message);
                    if (this._debug) this.DebugErrors(ex);
                }
            }

            //Then we'll apply merging if applicable
            //If merge was false then we've already done the outputting as we had no need to keep
            //stuff in memory
            if (this._merge)
            {
                if (this._storeWriter != null && (this._writer == null || this._dataset))
                {
                    //We only have a StoreWriter so we output a Dataset rather than merging
                    if (!this._output.Equals(String.Empty))
                    {
                        if (File.Exists(this._output) && !this._overwrite)
                        {
                            Console.Error.WriteLine("rdfConvert: Unable to output to '" + this._output + "' because a file already exists at this location and the -overwrite argument was not used");
                        }
                        else
                        {
                            try
                            {
                                this._storeWriter.Save(this._store, new VDS.RDF.Storage.Params.StreamParams(this._output));
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine("rdfConvert: Unable to output to '" + this._output + "' due to the following error:");
                                Console.Error.WriteLine("rdfConvert: Error: " + ex.Message);
                                if (this._debug) this.DebugErrors(ex);
                            }
                        }
                    }
                    else
                    {
                        String destFile = (this._inputs.Count == 1 && !this._inputs[0].StartsWith("-uri:")) ? Path.GetFileNameWithoutExtension(this._inputs[0]) + this._outExt : "dataset" + this._outExt;
                        if (File.Exists(destFile) && !this._overwrite)
                        {
                            Console.Error.WriteLine("rdfConvert: Unable to output to '" + destFile + "' because a file already exists at this location and the -overwrite argument was not used");
                        }
                        else
                        {
                            try
                            {
                                this._storeWriter.Save(this._store, new VDS.RDF.Storage.Params.StreamParams(destFile));
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine("rdfConvert: Unable to output to '" + destFile + "' due to the following error:");
                                Console.Error.WriteLine("rdfConvert: Error: " + ex.Message);
                                if (this._debug) this.DebugErrors(ex);
                            }
                        }
                     }
                }
                else
                {
                    //Merge all the Graphs together and produce a single Graph
                    Graph mergedGraph = new Graph();
                    foreach (IGraph g in this._store.Graphs)
                    {
                        mergedGraph.Merge(g);
                    }

                    //Work out the output file and output the Graph
                    String destFile;
                    if (!this._output.Equals(String.Empty))
                    {
                        destFile = this._output;
                    } 
                    else 
                    {
                        destFile = "merged-graph" + this._outExt;
                    }
                    if (File.Exists(destFile) && !this._overwrite)
                    {
                        Console.Error.WriteLine("rdfConvert: Unable to output to '" + destFile + "' because a file already exists at this location and the -overwrite argument was not used");
                    }
                    else
                    {
                        try
                        {
                            this._writer.Save(mergedGraph, destFile);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("rdfConvert: Unable to output to '" + destFile + "' due to the following error:");
                            Console.Error.WriteLine("rdfConvert: Error: " + ex.Message);
                            if (this._debug) this.DebugErrors(ex);
                        }
                    }
                }
            }
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
                if (arg.StartsWith("-uri:"))
                {
                    this._inputs.Add(arg);
                }
                else if (arg.StartsWith("-hs"))
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
                else if (arg.Equals("-merge"))
                {
                    this._merge = true;
                }
                else if (arg.Equals("-overwrite"))
                {
                    this._overwrite = true;
                }
                else if (arg.Equals("-dataset"))
                {
                    this._dataset = true;
                    this._merge = true;
                }
                else if (arg.StartsWith("-out:") || arg.StartsWith("-output:"))
                {
                    this._output = arg.Substring(arg.IndexOf(':') + 1);
                    //If the Writers have not been set then we'll set them now
                    if (this._writer == null && this._storeWriter == null)
                    {
                        String format;
                        try
                        {
                            format = MimeTypesHelper.GetMimeType(Path.GetExtension(this._output));
                        }
                        catch (RdfException)
                        {
                            Console.Error.WriteLine("rdfConvert: The File Extension '" + Path.GetExtension(this._output) + "' is not permissible since dotNetRDF cannot infer a MIME type from the extension");
                            return false;
                        }
                        try
                        {
                            this._writer = MimeTypesHelper.GetWriter(format);
                        }
                        catch (RdfException)
                        {
                            //Supress this error
                        }
                        try
                        {
                            this._storeWriter = MimeTypesHelper.GetStoreWriter(format);
                            if (this._writer == null)
                            {
                                this._merge = true;
                            }
                            else if (this._writer is NTriplesWriter && !Path.GetExtension(this._output).Equals(".nt"))
                            {
                                this._writer = null;
                                this._merge = true;
                            }
                        }
                        catch (RdfException)
                        {
                            //Suppress this error
                        }
                        if (this._writer == null && this._storeWriter == null)
                        {
                            Console.Error.WriteLine("rdfConvert: The MIME Type '" + format + "' is not permissible since dotNetRDF does not support outputting in that format");
                            return false;
                        }
                    }
                }
                else if (arg.StartsWith("-outformat:"))
                {
                    String format = arg.Substring(arg.IndexOf(':') + 1);
                    if (!format.Contains("/"))
                    {
                        try
                        {
                            format = MimeTypesHelper.GetMimeType(format);
                        }
                        catch (RdfException)
                        {
                            Console.Error.WriteLine("rdfConvert: The File Extension '" + format + "' is not permissible since dotNetRDF cannot infer a MIME type from the extension");
                            return false;
                        }
                    }
                    //Validate the MIME Type
                    if (!IsValidMimeType(format))
                    {
                        Console.Error.WriteLine("rdfConvert: The MIME Type '" + format + "' is not permissible since dotNetRDF does not support outputting in that format");
                        return false;
                    }
                    try
                    {
                        this._writer = MimeTypesHelper.GetWriter(format);
                        this._outExt = MimeTypesHelper.GetFileExtension(this._writer);
                    }
                    catch (RdfException)
                    {
                        //Supress this error
                    }
                    try
                    {
                        this._storeWriter = MimeTypesHelper.GetStoreWriter(format);
                        if (this._writer == null)
                        {
                            //In the event that we can't get a valid Writer then individual graphs
                            //will be put into a Store and output as a Dataset
                            this._merge = true;
                            this._outExt = MimeTypesHelper.GetFileExtension(this._storeWriter);
                        }
                        else if (this._writer is NTriplesWriter && (!format.Equals("nt") || !format.Equals(".nt") || !format.Equals("text/plain")))
                        {
                            this._writer = null;
                            this._merge = true;
                            this._outExt = MimeTypesHelper.GetFileExtension(this._storeWriter);
                        }
                    }
                    catch (RdfException)
                    {
                        //Suppress this error
                    }
                    if (this._writer == null && this._storeWriter == null)
                    {
                        Console.Error.WriteLine("rdfConvert: The MIME Type '" + format + "' is not permissible since dotNetRDF does not support outputting in that format");
                        return false;
                    }
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
                    //Anything else is treated as an input file
                    this._inputs.Add(arg);
                }
            }

            //If there are no this._inputs then we'll abort
            if (this._inputs.Count == 0)
            {
                Console.Error.WriteLine("rdfConvert: No Inputs were provided - please provide one/more files or URIs you wish to convert");
                return false;
            }

            //If there are no writers specified then we'll abort
            if (this._writer == null && this._storeWriter == null)
            {
                Console.Error.WriteLine("rdfConvert: Aborting since no output options have been specified, use the -out:filename or -outformat: arguments to specify output format");
                return false;
            }

            if (!this._outExt.Equals(String.Empty))
            {
                if (!this._outExt.StartsWith(".")) this._outExt = "." + this._outExt;
            }
            else if (!this._output.Equals(String.Empty))
            {
                this._outExt = Path.GetExtension(this._output);
            }

            //Apply the Options to the Writers
            foreach (IConversionOption option in this._options)
            {
                if (this._writer != null) option.Apply(this._writer);
                if (this._storeWriter != null) option.Apply(this._storeWriter);
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
            Console.WriteLine("e.g. rdfConvert input1.rdf input2.ttl input3.n3 -outformat:text/html");
            Console.WriteLine();
            Console.WriteLine("You can use URIs as input by prefixing the uri with the -uri: flag e.g.");
            Console.WriteLine("rdfConvert -uri:http://example.org/something -out:something.rdf");
            Console.WriteLine();
            Console.WriteLine("Notes");
            Console.WriteLine("-----");
            Console.WriteLine();
            Console.WriteLine("1. If you retrieve a single URI then you must use the -out:filename.ext option to set the file to output to");
            Console.WriteLine("2. If your inputs are a mixture of Graphs and Datasets and the output format may be used for Graphs/Datasets (e.g. CSV) then you must specify the -dataset option to batch convert datasets or each graph in the dataset will be output individually");
            Console.WriteLine();
            Console.WriteLine("Supported Options");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine(" -c[:integer]");
            Console.WriteLine(" Sets the Compression Level used by compressing writers, if specified without an integer parameter then defaults to default compression");
            Console.WriteLine();
            Console.WriteLine(" -dataset");
            Console.WriteLine(" Specifies that all Graphs retrieved from input should be output as an RDF Dataset");
            Console.WriteLine();
            Console.WriteLine(" -debug");
            Console.WriteLine(" Prints more detailed error messages if errors occur");
            Console.WriteLine();
            Console.WriteLine(" -help");
            Console.WriteLine(" Prints this usage summary if it is the only argument, otherwise ignored");
            Console.WriteLine();
            Console.WriteLine(" -hs[:(true|false)]");
            Console.WriteLine(" Enables/Disables High Speed write mode, if specified without a boolean parameter then defaults to enabled");
            Console.WriteLine();
            Console.WriteLine(" -merge");
            Console.WriteLine(" Specifies that all Graphs retrieved from input should be merged into a single Graph and output as such");
            Console.WriteLine();
            Console.WriteLine(" -nocache");
            Console.WriteLine(" Specifies that UriLoader caching is disabled");
            Console.WriteLine();
            Console.WriteLine(" -out:filename.ext");
            Console.WriteLine(" -output:filename.ext");
            Console.WriteLine(" Specifies a specific file to output to, if more than one input is specified then this parameter may not be honoured depending on the inputs and other options");
            Console.WriteLine();
            Console.WriteLine(" -outext:ext");
            Console.WriteLine(" Overrides the default file extension which will be automatically determined based on the -out/-outformat option.  Must occur after the -out/-outformat option or it may be ignored");
            Console.WriteLine();
            Console.WriteLine(" -outformat:(mime/type|ext)");
            Console.WriteLine(" Specifies an output format in terms of a MIME Type (or a file extension), if the MIME type/file extension does not correspond to a supported RDF Graph/Dataset format then the utility aborts.");
            Console.WriteLine();
            Console.WriteLine(" -overwrite");
            Console.WriteLine(" Specifies that the utility can overwrite existing files");
            Console.WriteLine();
            Console.WriteLine(" -pp[:(true|false)]");
            Console.WriteLine(" Enables/Disables Pretty Printing, if specified without a boolean parameter then defaults to enabled");
            Console.WriteLine();
            Console.WriteLine(" -rapper");
            Console.WriteLine(" Runs rdfConvert in rapper compatibility mode, type rdfConvert -rapper -h for further information.  Must be the first argument or ignored");
            Console.WriteLine();
            Console.WriteLine(" -warnings");
            Console.WriteLine(" Shows Warning Messages output by Parsers and Serializers");
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
