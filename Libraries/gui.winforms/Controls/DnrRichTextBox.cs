using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace VDS.RDF.GUI.WinForms
{
    /// <summary>
    /// Extended richtextbox to allow disable/enable of updates
    /// </summary>
    public class DnrRichTextBox : RichTextBox
    {
        //note - http://stackoverflow.com/questions/3282384/richtextbox-syntax-highlighting-in-real-time-disabling-the-repaint

        public void BeginUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        }
        public void EndUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            this.Invalidate();
        }
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int WM_SETREDRAW = 0x0b;
    }
}
