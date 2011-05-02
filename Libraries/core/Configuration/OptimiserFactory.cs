using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Configuration
{
    public class OptimiserFactory : IObjectFactory
    {
        private const String QueryOptimiserDefault = "VDS.RDF.Query.Optimisation.DefaultOptimiser";
        private const String QueryOptimiserNoReorder = "VDS.RDF.Query.Optimisation.NoReorderOptimiser";
        private const String QueryOptimiserWeighted = "VDS.RDF.Query.Optimisation.WeightedOptimiser";

        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            Object temp;

            switch (targetType.FullName)
            {
                case QueryOptimiserDefault:
                    obj = new DefaultOptimiser();
                    break;

                case QueryOptimiserNoReorder:
                    obj = new NoReorderOptimiser();
                    break;

                case QueryOptimiserWeighted:
                    INode statsObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingGraph));
                    if (statsObj != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, statsObj);
                        if (temp is IGraph)
                        {
                            obj = new WeightedOptimiser((IGraph)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to create the Weighted Query Optimiser identified by the Node '" + objNode.ToString() + "' since the dnr:usingGraph property points to an object that cannot be loaded as an Object that imlements the required IGraph interface");
                        }
                    }
                    else
                    {
                        obj = new WeightedOptimiser();
                    }
                    break;

                default:
                    //Try and create an Algebra Optimiser
                    try
                    {
                        obj = (IAlgebraOptimiser)Activator.CreateInstance(targetType);
                    }
                    catch
                    {
                        //Any error means this loader can't load this type
                        return false;
                    }
                    break;
            }

            //Return true only if we've loaded something into the output object
            if (obj != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case QueryOptimiserDefault:
                case QueryOptimiserNoReorder:
                case QueryOptimiserWeighted:
                    return true;
                default:
                    Type algOptType = typeof(IAlgebraOptimiser);
                    return t.GetInterfaces().Any(i => i.Equals(algOptType));
            }
        }
    }
}
