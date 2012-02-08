using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.Alexandria.Documents.Adaptors;
using VDS.Alexandria.Documents.GraphRegistry;

namespace VDS.Alexandria.Documents
{
    public class FileDocumentManager : BaseDocumentManager<StreamReader, TextWriter>
    {
        private String _directory;
        private IGraphRegistry _graphRegistry;

        private static String[] RequiredDirectories = new String[] {
            @"index\",
            @"index\s\",
            @"index\p\",
            @"index\o\",
            @"index\sp\",
            @"index\so\",
            @"index\po\",
            @"index\spo\"
        };

        private const String GraphRegistryDocument = "graphs";

        public FileDocumentManager(String directory)
            : base(new NTriplesAdaptor())
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

            if (!this.HasDocument(GraphRegistryDocument))
            {
                if (!this.CreateDocument(GraphRegistryDocument))
                {
                    throw new AlexandriaException("Unable to create the Required Graph Registry Document");
                }
            }

            this._graphRegistry = new TsvGraphRegistry(this.GetDocument(GraphRegistryDocument));
        }

        protected override bool HasDocumentInternal(string name)
        {
            return File.Exists(Path.Combine(this._directory, name));
        }

        protected override bool CreateDocumentInternal(string name)
        {
            try
            {
                FileStream temp = File.Create(Path.Combine(this._directory, name));
                temp.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override bool DeleteDocumentInternal(string name)
        {
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

        protected override IDocument<StreamReader,TextWriter> GetDocumentInternal(string name)
        {
            if (File.Exists(Path.Combine(this._directory, name)))
            {
                return new FileDocument(Path.Combine(this._directory, name), name, this);
            }
            else
            {
                throw new AlexandriaException("The requested Document " + name + " is not present in this Store");
            }
        }

        public override IGraphRegistry GraphRegistry
        {
            get 
            {
                return this._graphRegistry;
            }
        }
    }
}
