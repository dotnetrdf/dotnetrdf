using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.Web
{
    public class SimpleFileHttpHandlers : HttpListenerHandlerCollection
    {
        private String _baseDirectory;

        public SimpleFileHttpHandlers(String baseDirectory)
        {
            if (baseDirectory == null) throw new ArgumentNullException("baseDirectory", "Cannot create a SimpleFileHttpHandlers collection with a null Base Directory");
            if (baseDirectory.Equals(String.Empty)) throw new ArgumentException("Cannot create a SimpleFileHttpHandlersCollection with an empty Base Directory", "baseDirectory");
            if (!Directory.Exists(baseDirectory)) throw new ArgumentException("Cannot create a SimpleFileHttpHandlers collection which serves a non-existent Base Directory", "baseDirectory");
            this._baseDirectory = baseDirectory;
            if (!this._baseDirectory.EndsWith(new String(new char[] {Path.DirectorySeparatorChar}))) this._baseDirectory += Path.DirectorySeparatorChar;
            
            //Add the Single Mapping we need
            this.AddMapping(new HttpRequestMapping(HttpRequestMapping.AllVerbs, "*", typeof(FileHttpHandler)));
        }

        public override void Initialise(HttpServerState state)
        {
            state["BaseDirectory"] = this._baseDirectory;
        }
    }
}
