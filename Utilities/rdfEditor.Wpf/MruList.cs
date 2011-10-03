using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    public class MruList
    {
        private List<String> _files = new List<string>();
        private String _file;
        private int _size;

        public const int DefaultSize = 9;
        public const int DefaultShortFilenameLength = 50;

        public MruList(String file, int size)
        {
            if (file == null) throw new ArgumentNullException("file");
            this._file = file;
            this._size = Math.Max(1, size);
            this.Load();
        }

        public MruList(String file)
            : this(file, DefaultSize) { }

        public IEnumerable<String> Files
        {
            get
            {
                return (from i in Enumerable.Range(0, this._files.Count).Reverse()
                        select this._files[i]);
            }
        }

        public void Load()
        {
            if (File.Exists(this._file))
            {
                using (StreamReader reader = new StreamReader(this._file))
                {
                    while (!reader.EndOfStream)
                    {
                        String line = reader.ReadLine();
                        if (line.Equals(String.Empty)) continue;
                        if (File.Exists(line))
                        {
                            this._files.Add(line);
                        }
                    }
                    reader.Close();
                }

                while (this._files.Count > this._size)
                {
                    this._files.RemoveAt(0);
                }
            }
        }

        public void Add(String file)
        {
            this._files.Remove(file);
            this._files.Add(file);
            this.Save();
        }

        public void Remove(String file)
        {
            this._files.Remove(file);
            this.Save();
        }

        public void Save()
        {
            while (this._files.Count > this._size)
            {
                this._files.RemoveAt(0);
            }

            using (StreamWriter writer = new StreamWriter(this._file))
            {
                foreach (String file in this._files)
                {
                    writer.WriteLine(file);
                }
                writer.Close();
            }
        }

        public void Clear()
        {
            this._files.Clear();
            this.Save();
        }

        public static String ShortenFilename(String file)
        {
            return ShortenFilename(file, DefaultShortFilenameLength);
        }

        public static String ShortenFilename(String file, int maxLength)
        {
            if (file.Length <= maxLength)
            {
                return file;
            }
            else
            {
                StringBuilder shortFile = new StringBuilder();
                String filename = Path.GetFileName(file);

                String sepChar = new String(new char[] { Path.DirectorySeparatorChar });
                String path = file.Substring(0, file.Length - filename.Length);
                if (path.Length == 0) return filename;
                String[] dirs = path.Split(new char[] { Path.DirectorySeparatorChar });

                int i = -1;
                while (i < dirs.Length && shortFile.Length + filename.Length < maxLength)
                {
                    if (shortFile.Length + filename.Length + dirs[i+1].Length >= maxLength) break;
                    i++;
                    shortFile.Append(dirs[i]);
                    shortFile.Append(sepChar);
                }
                if (i < dirs.Length - 1)
                {
                    shortFile.Append("...");
                    shortFile.Append(sepChar);
                }
                shortFile.Append(filename);
                return shortFile.ToString();
            }
        }
    }
}
