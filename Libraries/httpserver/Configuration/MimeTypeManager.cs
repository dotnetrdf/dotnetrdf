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

        public MimeTypeManager(bool useDefaults)
        {
            if (useDefaults) this.InitialiseWithDefaults();
        }

        public MimeTypeManager(XmlNode node)
        {
            if (node.Name.Equals("mimeTypes"))
            {
                //Use Defaults?
                //Do this first so user specified ones can override these settings
                String useDefaults = node.Attributes.GetSafeNamedItem("usedefault");
                if (useDefaults != null)
                {
                    if (useDefaults.ToLower().Equals("true")) this.InitialiseWithDefaults();
                }

                foreach (XmlNode mimeType in node.ChildNodes)
                {
                    if (mimeType.NodeType != XmlNodeType.Element) continue;

                    if (mimeType.Name.Equals("mimeType"))
                    {
                        String ext = mimeType.Attributes.GetSafeNamedItem("extension");
                        if (ext == null) throw new HttpServerException("Configuration File is invalid since a <mimeType> element does not have the required extension attribute");
                        String type = mimeType.Attributes.GetSafeNamedItem("type");
                        if (type == null) throw new HttpServerException("Configuration File is invalid since a <mimeTyoe> element does not have the required type attribute");
                        String bin = mimeType.Attributes.GetSafeNamedItem("binary");
                        if (bin == null) bin = "false";
                        bin = bin.ToLower();

                        this.AddMimeType(new MimeTypeMapping(ext, type, bin.Equals("true")));
                    }
                    else
                    {
                        throw new HttpServerException("Configuration File is invalid since the <mimeTypes> element contains an unexpected <" + mimeType.Name + "> element");
                    }
                }
            }
            else
            {
                throw new HttpServerException("MimeTypeManager constructor expected to receive a <mimeTypes> node but received a <" + node.Name + "> node");
            }
        }

        private void InitialiseWithDefaults()
        {
            this.AddMimeType(new MimeTypeMapping(".html", "text/html"));
            this.AddMimeType(new MimeTypeMapping(".htm", "text/html"));
            this.AddMimeType(new MimeTypeMapping(".xhtml", "application/xhtml+xml"));
            this.AddMimeType(new MimeTypeMapping(".xml", "application/xml"));
            this.AddMimeType(new MimeTypeMapping(".css", "text/css"));
            this.AddMimeType(new MimeTypeMapping(".js", "text/javascript"));
            this.AddMimeType(new MimeTypeMapping(".txt", "text/plain"));
            this.AddMimeType(new MimeTypeMapping(".csv", "text/csv"));
            this.AddMimeType(new MimeTypeMapping(".tsv", "text/tab-separated-values"));
            this.AddMimeType(new MimeTypeMapping(".jpg", "image/jpeg", true));
            this.AddMimeType(new MimeTypeMapping(".jpeg", "image/jpeg", true));
            this.AddMimeType(new MimeTypeMapping(".gif", "image/gif", true));
            this.AddMimeType(new MimeTypeMapping(".png", "image/png", true));
            this.AddMimeType(new MimeTypeMapping(".bmp", "image/bmp", true));
            this.AddMimeType(new MimeTypeMapping(".ico", "image/vnd.microsoft.icon", true));
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
