/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace VDS.RDF.Utilities.Server.GUI
{
    /// <summary>
    /// User Control for displaying Server Monitoring information
    /// </summary>
    public partial class ServerMonitor 
        : UserControl
    {
        private List<String> _buffer = new List<string>();
        private int _size = 50;

        /// <summary>
        /// Creates a new Server Monitor
        /// </summary>
        public ServerMonitor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Configures the Buffer Size for the Monitor
        /// </summary>
        [EditorBrowsable(),Description("Sets the maximum number of lines of output to display in the Monitor")]
        public int BufferSize
        {
            get
            {
                return this._size;
            }
            set
            {
                if (value > 0) this._size = value;
            }
        }

        /// <summary>
        /// Clears the monitor
        /// </summary>
        public void Clear()
        {
            this.txtView.Text = String.Empty;
            this._buffer.Clear();
        }

        /// <summary>
        /// Adds a line to the monitor
        /// </summary>
        /// <param name="line"></param>
        public void WriteLine(String line)
        {
            if (line == null) return;
            if (line.Contains("\r\n"))
            {
                this._buffer.AddRange(line.Split(new char[] {'\r', '\n'}));
            }
            else
            {
                this._buffer.Add(line);
            }
            this.AppendText(this.txtView, "\r\n" + line);
            if (this._buffer.Count > this._size)
            {
                while (this._buffer.Count > this._size)
                {
                    this._buffer.RemoveAt(0);
                }
                this.SetText(this.txtView, String.Join("\r\n", this._buffer.ToArray()));
            }
        }

        /// <summary>
        /// Delegate for setting the text of the monitor
        /// </summary>
        /// <param name="tbox">Text Box</param>
        /// <param name="text">Text</param>
        public delegate void SetTextDelegate(TextBox tbox, String text);

        /// <summary>
        /// Delegate for appending text to the monitor
        /// </summary>
        /// <param name="tbox">Text Box</param>
        /// <param name="text">Text</param>
        public delegate void AppendTextDelegate(TextBox tbox, String text);

        /// <summary>
        /// Sets the text of the monitor
        /// </summary>
        /// <param name="tbox">Text Box</param>
        /// <param name="text">Text</param>
        public void SetText(TextBox tbox, String text)
        {
            if (this.InvokeRequired)
            {
                SetTextDelegate d = new SetTextDelegate(this.SetText);
                this.Invoke(d, new object[] { tbox, text });
            }
            else
            {
                tbox.Text = text;
                tbox.SelectionStart = text.Length - 1;
                tbox.ScrollToCaret();
            }
        }

        /// <summary>
        /// Appends text to the monitor
        /// </summary>
        /// <param name="tbox">Text Box</param>
        /// <param name="text">Text</param>
        public void AppendText(TextBox tbox, String text)
        {
            if (this.InvokeRequired)
            {
                AppendTextDelegate d = new AppendTextDelegate(this.AppendText);
                this.Invoke(d, new object[] { tbox, text });
            }
            else
            {
                tbox.Text += text;
                tbox.SelectionStart = tbox.Text.Length - 1;
                tbox.ScrollToCaret();
            }
        }
    }
}
