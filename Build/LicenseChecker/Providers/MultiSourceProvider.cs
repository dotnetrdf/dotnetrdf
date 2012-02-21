using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LicenseChecker.Providers
{
    class MultiSourceProvider
        : ISourceProvider
    {
        private List<ISourceProvider> _providers = new List<ISourceProvider>();

        public MultiSourceProvider(IEnumerable<ISourceProvider> providers)
        {
            this._providers.AddRange(providers);
        }

        public IEnumerable<string> GetSourceFiles()
        {
            return (from p in this._providers
                    from f in p.GetSourceFiles()
                    select f);
        }
    }
}
