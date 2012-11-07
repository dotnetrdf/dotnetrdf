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
