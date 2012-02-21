using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LicenseChecker.Providers
{
    abstract class RegexFilteringProvider
        : ISourceProvider
    {
        private ISourceProvider _provider;
        private Regex _regex;
        private bool _exclude = false;

        public RegexFilteringProvider(ISourceProvider provider, String pattern, bool exclude)
        {
            this._provider = provider;
            this._regex = new Regex(pattern);
            this._exclude = exclude;
        }

        public RegexFilteringProvider(ISourceProvider provider, String pattern)
            : this(provider, pattern, false) { }

        public IEnumerable<string> GetSourceFiles()
        {
            if (this._exclude)
            {
                return this._provider.GetSourceFiles().Where(f => !this._regex.IsMatch(f));
            }
            else
            {
                return this._provider.GetSourceFiles().Where(f => this._regex.IsMatch(f));
            }
        }
    }

    class RegexInclusionProvider
        : RegexFilteringProvider
    {
        public RegexInclusionProvider(ISourceProvider provider, String pattern)
            : base(provider, pattern, false) { }
    }

    class RegexExclusionProvider
    : RegexFilteringProvider
    {
        public RegexExclusionProvider(ISourceProvider provider, String pattern)
            : base(provider, pattern, true) { }
    }
}
