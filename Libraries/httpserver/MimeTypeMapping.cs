using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Web
{
    public class MimeTypeMapping
    {
        private String _extension;
        private String _mimeType;
        private bool _binary = false;

        public MimeTypeMapping(String extension, String mimeType)
            : this(extension, mimeType, false) { }

        public MimeTypeMapping(String extension, String mimeType, bool binary)
        {
            this._extension = extension;
            if (!this._extension.StartsWith(".")) this._extension = "." + this._extension;
            if (!mimeType.Contains("/")) throw new ArgumentException("'" + mimeType + "' is not a valid Mime Type", "mimeType");
            this._mimeType = mimeType;
            this._binary = binary;
        }

        public String Extension
        {
            get
            {
                return this._extension;
            }
        }

        public String MimeType
        {
            get
            {
                return this._mimeType;
            }
        }

        public bool IsBinaryData
        {
            get
            {
                return this._binary;
            }
        }
    }
}
