using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LicenseChecker.Providers
{
    abstract class ExtensionFilteringProvider
        : ISourceProvider
    {
        private ISourceProvider _provider;
        private HashSet<String> _extensions;
        private bool _exclude = false;

        public ExtensionFilteringProvider(ISourceProvider provider, IEnumerable<String> extensions, bool exclude)
        {
            this._provider = provider;
            this._extensions = new HashSet<string>(extensions.Where(e => e.Length > 0).Select(e => e.StartsWith(".") ? e.Substring(1).ToLower() : e.ToLower()));
            this._exclude = exclude;
        }

        public ExtensionFilteringProvider(ISourceProvider provider, IEnumerable<String> extensions)
            : this(provider, extensions, false) { }

        public IEnumerable<string> GetSourceFiles()
        {
            if (this._exclude)
            {
                return this._provider.GetSourceFiles().Where(f => Path.GetExtension(f).Length > 0 && !this._extensions.Contains(Path.GetExtension(f).Substring(1).ToLower()));
            }
            else
            {
                return this._provider.GetSourceFiles().Where(f => Path.GetExtension(f).Length > 0 && this._extensions.Contains(Path.GetExtension(f).Substring(1).ToLower()));
            }
        }
    }

    class ExtensionInclusionProvider
        : ExtensionFilteringProvider
    {
        public ExtensionInclusionProvider(ISourceProvider provider, IEnumerable<String> extensions)
            : base(provider, extensions, false) { }
    }

    class ExtensionExclusionProvider
    : ExtensionFilteringProvider
    {
        public ExtensionExclusionProvider(ISourceProvider provider, IEnumerable<String> extensions)
            : base(provider, extensions, true) { }
    }
}
