using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Management.Provisioning.Sesame.Owlim
{
    public enum OwlimRuleset
    {
        NONE,
        RDFS,
        RDFS_Optimized,
        OWL_Horst,
        OWL_Horst_Optimized,
        OWL_Max,
        OWL_Max_Optimized,
        OWL2_RL_Conf,
        OWL2_RL_Reduced,
        OWL2_RL_Reduced_Optimized
    }

    class BaseOwlimTemplate
    {
        public const String OwlimNamespace = "http://www.ontotext.com/trree/owlim#";

    }
}
