using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Alexandria.Documents.Adaptors;

namespace Alexandria.Documents
{
    public class FileDocumentManager : IDocumentManager
    {
        private String _directory;
        private Dictionary<String,DocumentReference> _activeDocuments = new Dictionary<string,DocumentReference>();
        private NTriplesAdaptor _adaptor = new NTriplesAdaptor();

        private static String[] RequiredDirectories = new String[] {
            @"index\",
            @"index\s\",
            @"index\p\",
            @"index\o\",
            @"index\sp\",
            @"index\so\",
            @"index\po\"
        };

        public FileDocumentManager(String directory)
        {
            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    throw new AlexandriaException("Unable to create the Directory " + directory + " for use as an Alexandria Store", ex);
                }
            }
            this._directory = directory;
            if (!this._directory.EndsWith("\\")) this._directory += "\\";

            //Ensure relevant other directories exist
            foreach (String dir in RequiredDirectories)
            {
                try
                {
                    if (!Directory.Exists(Path.Combine(this._directory, dir)))
                    {
                        Directory.CreateDirectory(Path.Combine(this._directory, dir));
                    }
                }
                catch (Exception ex)
                {
                    throw new AlexandriaException("Unable to create the Required Directory " + dir + " for use as part of an Alexandria Store", ex);
                }
            }
        }

        public IDocumentToGraphAdaptor GraphAdaptor
        {
            get
            {
                return this._adaptor;
            }
        }

        public bool HasDocument(string name)
        {
            if (this._activeDocuments.ContainsKey(name))
            {
                return true;
            }
            else
            {
                return File.Exists(Path.Combine(this._directory, name));
            }
        }

        public bool CreateDocument(string name)
        {
            if (this._activeDocuments.ContainsKey(name))
            {
                return false;
            }
            else if (File.Exists(Path.Combine(this._directory, name)))
            {
                return false;
            }
            else
            {
                try
                {
                    File.Create(Path.Combine(this._directory, name));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool DeleteDocument(string name)
        {
            if (this._activeDocuments.ContainsKey(name))
            {
                if (this._activeDocuments[name].ReferenceCount > 0)
                {
                    return false;
                }
            }

            //Attempt to delete the actual document
            if (File.Exists(Path.Combine(this._directory, name)))
            {
                try
                {
                    File.Delete(Path.Combine(this._directory, name));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public IDocument GetDocument(string name)
        {
            if (this._activeDocuments.ContainsKey(name))
            {
                this._activeDocuments[name].IncrementReferenceCount();
                return this._activeDocuments[name].Document;
            }
            else
            {
                if (File.Exists(Path.Combine(this._directory, name)))
                {
                    FileDocument doc = new FileDocument(Path.Combine(this._directory, name));
                    this._activeDocuments.Add(name, new DocumentReference(doc));
                    this._activeDocuments[name].IncrementReferenceCount();
                    return doc;
                }
                else
                {
                    throw new AlexandriaException("The request Document " + name + " is not present in this Store");
                }
            }
        }

        public bool ReleaseDocument(String name)
        {
            if (this._activeDocuments.ContainsKey(name))
            {
                this._activeDocuments[name].DecrementReferenceCount();
                if (this._activeDocuments[name].ReferenceCount == 0)
                {
                    this._activeDocuments.Remove(name);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            foreach (DocumentReference reference in this._activeDocuments.Values)
            {
                reference.Dispose();
            }
        }
    }
}
