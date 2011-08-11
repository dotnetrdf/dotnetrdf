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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_PROCESS

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
            this.LocateGraphViz();
            this._format = format;
        }

        /// <summary>
        /// Creates a new GraphVizGenerator
        /// </summary>
        /// <param name="format">Format for the Output</param>
        /// <param name="gvdir">Directory in which GraphViz is installed</param>
        public GraphVizGenerator(String format, String gvdir) : this(format)
        {
            if (gvdir.LastIndexOf('\\') != gvdir.Length)
            {
                this._graphvizdir = gvdir + "\\";
            }
            else
            {
                this._graphvizdir = gvdir;
            }
        }

        /// <summary>
        /// Gets/Sets the Format for the Output
        /// </summary>
        public String Format
        {
            get
            {
                return this._format;
            }
            set
            {
                this._format = value;
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
            //Prepare the Process
            ProcessStartInfo start = new ProcessStartInfo();
            if (!_graphvizdir.Equals(String.Empty)) {
                start.FileName = this._graphvizdir + "dot.exe";
            } else {
                start.FileName = "dot.exe";
            }
            start.Arguments = "-T" + this._format;
            start.UseShellExecute = false;
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;

            //Prepare the GraphVizWriter and Streams
            GraphVizWriter gvzwriter = new GraphVizWriter();
            BinaryWriter output = new BinaryWriter(new FileStream(filename, FileMode.Create));

            //Start the Process
            Process gvz = new Process();
            gvz.StartInfo = start;
            gvz.Start();

            //Write to the Standard Input
            gvzwriter.Save(g, gvz.StandardInput);

            //Read the Standard Output
            Tools.StreamCopy(gvz.StandardOutput.BaseStream, output.BaseStream);
            output.Close();

            gvz.Close();

            //Open if requested
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
                    this._graphvizdir = folder;
                    return;
                } 
                else if (File.Exists(folder + "\\dot.exe")) 
                {
                    this._graphvizdir = folder + "\\";
                    return;
                }
            }
        }
    }
}

#endif