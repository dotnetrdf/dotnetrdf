using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace rdfServer.GUI
{
    public partial class ServerMonitor : UserControl
    {
        private List<String> _buffer = new List<string>();
        private int _size = 50;

        public ServerMonitor()
        {
            InitializeComponent();
        }

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

        public void Clear()
        {
            this.txtView.Text = String.Empty;
            this._buffer.Clear();
        }

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

        public delegate void SetTextDelegate(TextBox tbox, String text);

        public delegate void AppendTextDelegate(TextBox tbox, String text);

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
            }
        }

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
            }
        }
    }
}
