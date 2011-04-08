using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.LinkedData.Profiles
{
    /// <summary>
    /// Class for representing Expansion Linksets
    /// </summary>
    public class ExpansionLinkset : VoIDLinkset
    {
        private bool _ignore = false;
        private INode _subject;

        public ExpansionLinkset(IGraph expansionDescription, INode linksetSubj)
            : base(expansionDescription, linksetSubj) 
        {
            this._subject = linksetSubj;

            //Check for aat:ignoreLinkset
            IEnumerable<Triple> ts = expansionDescription.GetTriplesWithSubjectPredicate(linksetSubj, expansionDescription.CreateUriNode("aat:ignoreDataset"));
            Triple t = ts.FirstOrDefault();
            if (t != null)
            {
                if (t.Object.NodeType == NodeType.Literal)
                {
                    ILiteralNode l = (ILiteralNode)t.Object;
                    if (l.DataType != null && l.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                    {
                        this._ignore = Boolean.Parse(l.Value);
                    }
                }
            }
        }

        public ExpansionLinkset(INode linksetSubj, INode linkType)
            : base()
        {
            this._subject = linksetSubj;
            this._subjectTarget = linksetSubj;
            this._objectsTarget = linksetSubj;
            this._predicates.Add(linkType);
        }

        public bool Ignore
        {
            get
            {
                return this._ignore;
            }
        }

        public INode Subject
        {
            get
            {
                return this._subject;
            }
        }
    }
}
