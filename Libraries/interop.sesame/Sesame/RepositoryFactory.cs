using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;
using dotSesameRepo = org.openrdf.repository;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Interop.Sesame
{
    public class DotNetRdfRepositoryFactory : dotSesameRepo.config.RepositoryFactory
    {
        private int _nextID = 0;

        #region RepositoryFactory Members

        public org.openrdf.repository.config.RepositoryImplConfig getConfig()
        {
            return new DotNetRdfRepositoryConfiguration((++this._nextID).ToString());
        }

        public org.openrdf.repository.Repository getRepository(org.openrdf.repository.config.RepositoryImplConfig ric)
        {
            if (ric is DotNetRdfRepositoryConfiguration)
            {
                return ((DotNetRdfRepositoryConfiguration)ric).Repository;
            }
            else
            {
                throw new dotSesameRepo.config.RepositoryConfigException("This Factory does not know how to get repositories with Configurations of Type " + ric.GetType().Name);
            }
        }

        public string getRepositoryType()
        {
            return "VDS.RDF.Interop.Sesame.DotNetRdfInMemoryRepository";
        }

        #endregion
    }

    public class DotNetRdfRepositoryConfiguration : dotSesameRepo.config.RepositoryImplConfig
    {
        private String _name;
        private dotSesameRepo.Repository _repo;

        public DotNetRdfRepositoryConfiguration(String name)
        {
            this._name = name;
        }

        public void SetInMemoryStore(IInMemoryQueryableStore store)
        {
            this._repo = new DotNetRdfInMemoryRepository(store);
        }

        public void SetGenericStore(IGenericIOManager manager)
        {
            this._repo = new DotNetRdfGenericRepository(manager);
        }

        internal dotSesameRepo.Repository Repository
        {
            get
            {
                if (this._repo == null) throw new dotSesameRepo.config.RepositoryConfigException("This Configuration Object does not refer to an actual Repository");
                return this._repo;
            }
        }

        #region RepositoryImplConfig Members

        public org.openrdf.model.Resource export(org.openrdf.model.Graph g)
        {
            SesameGraph temp = new SesameGraph(g);
            if (this._repo is IConfigurationSerializable)
            {
                IUriNode subj = temp.CreateUriNode(new Uri("dotnetrdf:interop:sesame:repository:" + this._name));
                ConfigurationSerializationContext context = new ConfigurationSerializationContext(temp);
                context.NextSubject = subj;
                ((IConfigurationSerializable)this._repo).SerializeConfiguration(context);
                return g.getValueFactory().createURI(subj.ToString());                
            }
            else
            {
                throw new NotSupportedException("The underlying Repository does not support having it's Configuration serialized");
            }
        }

        public string getType()
        {
            return "VDS.RDF.Interop.Sesame.DotNetRdfInMemoryRepository";
        }

        public void parse(org.openrdf.model.Graph g, org.openrdf.model.Resource r)
        {
            Graph config = new Graph();
            SesameMapping mapping = new SesameMapping(config, g);
            SesameConverter.FromSesame(g, config);

            this._name = r.stringValue().Substring(r.stringValue().LastIndexOf(":") + 1);

            Object temp = ConfigurationLoader.LoadObject(config, SesameConverter.FromSesameResource(r, mapping));
            if (temp is IInMemoryQueryableStore)
            {
                this._repo = new DotNetRdfInMemoryRepository((IInMemoryQueryableStore)temp);
            }
            else if (temp is IGenericIOManager)
            {
                this._repo = new DotNetRdfGenericRepository((IGenericIOManager)temp);
            }
            else
            {
                throw new dotSesameRepo.config.RepositoryConfigException("Unable to load Configuration for the Repository as the loaded Object was not an IInMemoryQueryableStore or an IGenericIOManager implementation");
            }
        }

        public void validate()
        {
            //Does Nothing
        }

        #endregion
    }
}
