using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace VDS.Web.Configuration
{
    public class MimeTypeManager
    {
        private Dictionary<String, MimeTypeMapping> _mappings = new Dictionary<string, MimeTypeMapping>();

        public MimeTypeManager()
        {
            this.AddMimeType(new MimeTypeMapping(".html", "text/html"));
            this.AddMimeType(new MimeTypeMapping(".htm", "text/html"));
            this.AddMimeType(new MimeTypeMapping(".xhtml", "application/xhtml+xml"));
            this.AddMimeType(new MimeTypeMapping(".xml", "application/xml"));
            this.AddMimeType(new MimeTypeMapping(".css", "text/css"));
            this.AddMimeType(new MimeTypeMapping(".js", "text/javascript"));
            this.AddMimeType(new MimeTypeMapping(".txt", "text/plain"));
            this.AddMimeType(new MimeTypeMapping(".jpg", "image/jpeg", true));
            this.AddMimeType(new MimeTypeMapping(".jpeg", "image/jpeg", true));
            this.AddMimeType(new MimeTypeMapping(".gif", "image/gif", true));
            this.AddMimeType(new MimeTypeMapping(".png", "image/png", true));
            this.AddMimeType(new MimeTypeMapping(".bmp", "image/bmp", true));
        }

        public MimeTypeManager(XmlNode node)
        {

        }

        public void AddMimeType(MimeTypeMapping mapping)
        {
            if (this._mappings.ContainsKey(mapping.Extension))
            {
                this._mappings[mapping.Extension] = mapping;
            }
            else
            {
                this._mappings.Add(mapping.Extension, mapping);
            }
        }

        public void AddMimeType(String extension, String mimeType, bool binary)
        {
            this.AddMimeType(new MimeTypeMapping(extension, mimeType, binary));
        }

        public void AddMimeType(String extension, String mimeType)
        {
            this.AddMimeType(extension, mimeType, false);
        }

        public void RemoveMimeType(MimeTypeMapping mapping)
        {
            this.RemoveMimeType(mapping.Extension);
        }

        public void RemoveMimeType(String extension)
        {
            if (this._mappings.ContainsKey(extension))
            {
                this._mappings.Remove(extension);
            }
        }

        public MimeTypeMapping GetMapping(String extension)
        {
            if (this._mappings.ContainsKey(extension))
            {
                return this._mappings[extension];
            }
            else
            {
                return null;
            }
        }
    }
}
