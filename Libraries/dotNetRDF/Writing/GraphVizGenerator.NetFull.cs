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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// A Class which creates GraphViz Graphs entirely dynamically
    /// </summary>
    public class GraphVizGenerator
    {
        private String _format = "svg";
        private String _graphvizdir = String.Empty;

        /// <summary>
        /// Creates a new GraphVizGenerator
        /// </summary>
        /// <param name="format">Format for the Output (svg is default)</param>
        /// <remarks>Only use this form if you're certain that dot.exe is in your PATH otherwise the code will throw an error</remarks>
        public GraphVizGenerator(String format)
        {
            LocateGraphViz();
            _format = format;
        }

        /// <summary>
        /// Creates a new GraphVizGenerator
        /// </summary>
        /// <param name="format">Format for the Output</param>
        /// <param name="gvdir">Directory in which GraphViz is installed</param>
        public GraphVizGenerator(String format, String gvdir)
            : this(format)
        {
            if (gvdir.LastIndexOf('\\') != gvdir.Length)
            {
                _graphvizdir = gvdir + "\\";
            }
            else
            {
                _graphvizdir = gvdir;
            }
        }

        /// <summary>
        /// Gets/Sets the Format for the Output
        /// </summary>
        public String Format
        {
            get
            {
                return _format;
            }
            set
            {
                _format = value;
            }
        }

        /// <summary>
        /// Generates GraphViz Output for the given Graph
        /// </summary>
        /// <param name="g">Graph to generated GraphViz Output for</param>
        /// <param name="filename">File you wish to save the Output to</param>
        /// <param name="open">Whether you want to open the Output in the default application (according to OS settings) for the filetype after it is Created</param>
        public void Generate(IGraph g, String filename, bool open)
        {
            // Prepare the Process
            ProcessStartInfo start = new ProcessStartInfo();
            if (!_graphvizdir.Equals(String.Empty)) {
                start.FileName = _graphvizdir + "dot.exe";
            } else {
                start.FileName = "dot.exe";
            }
            start.Arguments = "-T" + _format;
            start.UseShellExecute = false;
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;

            // Prepare the GraphVizWriter and Streams
            GraphVizWriter gvzwriter = new GraphVizWriter();
            using (BinaryWriter writer = new BinaryWriter(new FileStream(filename, FileMode.Create)))
            {
                // Start the Process
                Process gvz = new Process();
                gvz.StartInfo = start;
                gvz.Start();

                // Write to the Standard Input
                gvzwriter.Save(g, gvz.StandardInput);

                // Read the Standard Output
                byte[] buffer = new byte[4096];
                using (BinaryReader reader = new BinaryReader(gvz.StandardOutput.BaseStream))
                {
                    while (true)
                    {
                        int read = reader.Read(buffer, 0, buffer.Length);
                        if (read == 0) break;
                        writer.Write(buffer, 0, read);
                    }
                    reader.Close();
                }
                writer.Close();
                gvz.Close();
            }

            // Open if requested
            if (open)
            {
                Process.Start(filename);
            }
        }

        /// <summary>
        /// Internal Helper Method for locating the GraphViz Directory using the PATH Environment Variable
        /// </summary>
        private void LocateGraphViz()
        {
            String path = Environment.GetEnvironmentVariable("path");
            String[] folders = path.Split(';');
            foreach (String folder in folders)
            {
                if (File.Exists(folder + "dot.exe"))
                {
                    _graphvizdir = folder;
                    return;
                } 
                else if (File.Exists(folder + "\\dot.exe")) 
                {
                    _graphvizdir = folder + "\\";
                    return;
                }
            }
        }
    }
}
