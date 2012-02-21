using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LicenseChecker.Providers
{
    abstract class NameFilteringProvider
        : ISourceProvider
    {
        private ISourceProvider _provider;
        private HashSet<String> _names;
        private bool _exclude = false;

        public NameFilteringProvider(ISourceProvider provider, IEnumerable<String> names, bool exclude)
        {
            this._provider = provider;
            this._names = new HashSet<string>(names.Select(n => Path.GetFileName(n)));
            this._exclude = exclude;
        }

        public NameFilteringProvider(ISourceProvider provider, IEnumerable<String> names)
            : this(provider, names, false) { }

        public IEnumerable<string> GetSourceFiles()
        {
            if (this._exclude)
            {
                return this._provider.GetSourceFiles().Where(f => !this._names.Contains(Path.GetFileName(f)));
            }
            else
            {
                return this._provider.GetSourceFiles().Where(f => this._names.Contains(Path.GetFileName(f)));
            }
        }
    }

    class NameInclusionProvider
        : NameFilteringProvider
    {
        public NameInclusionProvider(ISourceProvider provider, IEnumerable<String> names)
            : base(provider, names, false) { }
    }

    class NameExclusionProvider
        : NameFilteringProvider
    {
        public NameExclusionProvider(ISourceProvider provider, IEnumerable<String> names)
            : base(provider, names, true) { }
    }
}
