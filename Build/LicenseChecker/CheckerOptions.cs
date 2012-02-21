using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LicenseChecker.Providers;

namespace LicenseChecker
{
    class CheckerOptions
    {
        public CheckerOptions()
        {
            this.ExcludedExtensions = new List<string>();
            this.IncludedExtensions = new List<string>();
            this.IncludedFiles = new List<string>();
            this.ExcludedFiles = new List<string>();
            this.ProjectFiles = new List<string>();
            this.Directories = new List<string>();
        }

        public List<String> IncludedExtensions { get; set; }

        public List<String> ExcludedExtensions { get; set; }

        public List<String> IncludedFiles { get; set; }

        public List<String> ExcludedFiles { get; set; }

        public String InclusionPattern { get; set; }

        public String ExclusionPattern { get; set; }

        public List<String> ProjectFiles { get; set; }

        public List<String> Directories { get; set; }

        public String LicenseSearchString { get; set; }

        public ISourceProvider GetProvider()
        {
            List<ISourceProvider> providers = new List<ISourceProvider>();
            foreach (String project in this.ProjectFiles)
            {
                providers.Add(new ProjectProvider(project));
            }
            foreach (String dir in this.Directories)
            {
                providers.Add(new DirectoryProvider(dir, true));
            }

            ISourceProvider provider = new MultiSourceProvider(providers);

            if (this.IncludedExtensions.Count > 0) provider = new ExtensionInclusionProvider(provider, this.IncludedExtensions);
            if (this.IncludedFiles.Count > 0) provider = new NameInclusionProvider(provider, this.IncludedFiles);
            if (!String.IsNullOrEmpty(this.InclusionPattern)) provider = new RegexInclusionProvider(provider, this.InclusionPattern);
            if (this.ExcludedExtensions.Count > 0) provider = new ExtensionExclusionProvider(provider, this.ExcludedExtensions);
            if (this.ExcludedFiles.Count > 0) provider = new NameExclusionProvider(provider, this.ExcludedFiles);
            if (!String.IsNullOrEmpty(this.ExclusionPattern)) provider = new RegexExclusionProvider(provider, this.ExclusionPattern);

            return provider;
        }
    }
}
