using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Linq;
using System.Data.Linq;

namespace Some.Namespace
{


    public partial class MyOntologyDataContext : RdfDataContext
    {
        public MyOntologyDataContext(LinqTripleStore store) : base(store)
        {
        }
        public MyOntologyDataContext(string store) : base(new LinqTripleStore(store))
        {
        }

		        public IQueryable<Person> Persons
		        {
		            get
		            {
		                return ForType<Person>();
		            }
		        }
		
		        public IQueryable<Document> Documents
		        {
		            get
		            {
		                return ForType<Document>();
		            }
		        }
		
		        public IQueryable<Agent> Agents
		        {
		            get
		            {
		                return ForType<Agent>();
		            }
		        }
		
		        public IQueryable<Organization> Organizations
		        {
		            get
		            {
		                return ForType<Organization>();
		            }
		        }
		
		        public IQueryable<Project> Projects
		        {
		            get
		            {
		                return ForType<Project>();
		            }
		        }
		
		        public IQueryable<Group> Groups
		        {
		            get
		            {
		                return ForType<Group>();
		            }
		        }
		
		        public IQueryable<Image> Images
		        {
		            get
		            {
		                return ForType<Image>();
		            }
		        }
		
		        public IQueryable<PersonalProfileDocument> PersonalProfileDocuments
		        {
		            get
		            {
		                return ForType<PersonalProfileDocument>();
		            }
		        }
		
		        public IQueryable<OnlineAccount> OnlineAccounts
		        {
		            get
		            {
		                return ForType<OnlineAccount>();
		            }
		        }
		
		        public IQueryable<OnlineGamingAccount> OnlineGamingAccounts
		        {
		            get
		            {
		                return ForType<OnlineGamingAccount>();
		            }
		        }
		
		        public IQueryable<OnlineEcommerceAccount> OnlineEcommerceAccounts
		        {
		            get
		            {
		                return ForType<OnlineEcommerceAccount>();
		            }
		        }
		
		        public IQueryable<OnlineChatAccount> OnlineChatAccounts
		        {
		            get
		            {
		                return ForType<OnlineChatAccount>();
		            }
		        }
		

    }

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Person")]
public partial class Person : OwlInstanceSupertype
{
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "surname")]
  public string surname {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "family_name")]
  public string family_name {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "geekcode")]
  public string geekcode {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "firstName")]
  public string firstName {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "plan")]
  public string plan {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "knows")]
  public Person knows {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "img")]
  public Image img {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "name")]
  public string name { get; set; }
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "myersBriggs")]
  public string myersBriggs {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "workplaceHomepage")]
  public Document workplaceHomepage {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "workInfoHomepage")]
  public Document workInfoHomepage {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "schoolHomepage")]
  public Document schoolHomepage {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "interest")]
  public Document interest {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "topic_interest")]
  public VDS.RDF.Linq.OwlInstanceSupertype topic_interest {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "publications")]
  public Document publications {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "currentProject")]
  public VDS.RDF.Linq.OwlInstanceSupertype currentProject {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "pastProject")]
  public VDS.RDF.Linq.OwlInstanceSupertype pastProject {get;set;}

}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Document")]
public partial class Document : OwlInstanceSupertype
{
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "primaryTopic")]
  public VDS.RDF.Linq.OwlInstanceSupertype primaryTopic {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "topic")]
  public VDS.RDF.Linq.OwlInstanceSupertype topic {get;set;}

}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Agent")]
public partial class Agent : OwlInstanceSupertype
{
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "mbox_sha1sum")]
  public string mbox_sha1sum {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "gender")]
  public string gender {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "jabberID")]
  public string jabberID {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "aimChatID")]
  public string aimChatID {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "icqChatID")]
  public string icqChatID {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "yahooChatID")]
  public string yahooChatID {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "msnChatID")]
  public string msnChatID {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "birthday")]
  public string birthday {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "mbox")]
  public VDS.RDF.Linq.OwlInstanceSupertype mbox {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "weblog")]
  public Document weblog {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "openid")]
  public Document openid {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "tipjar")]
  public Document tipjar {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "made")]
  public VDS.RDF.Linq.OwlInstanceSupertype made {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "holdsAccount")]
  public OnlineAccount holdsAccount {get;set;}

}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Organization")]
public partial class Organization : OwlInstanceSupertype
{
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Project")]
public partial class Project : OwlInstanceSupertype
{
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Group")]
public partial class Group : OwlInstanceSupertype
{
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "member")]
  public Agent member {get;set;}

}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Image")]
public partial class Image : OwlInstanceSupertype
{
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "depicts")]
  public VDS.RDF.Linq.OwlInstanceSupertype depicts {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "thumbnail")]
  public Image thumbnail {get;set;}

}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="PersonalProfileDocument")]
public partial class PersonalProfileDocument : OwlInstanceSupertype
{
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="OnlineAccount")]
public partial class OnlineAccount : OwlInstanceSupertype
{
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "accountName")]
  public string accountName {get;set;}
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "accountServiceHomepage")]
  public Document accountServiceHomepage {get;set;}

}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="OnlineGamingAccount")]
public partial class OnlineGamingAccount : OwlInstanceSupertype
{
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="OnlineEcommerceAccount")]
public partial class OnlineEcommerceAccount : OwlInstanceSupertype
{
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="OnlineChatAccount")]
public partial class OnlineChatAccount : OwlInstanceSupertype
{
}



}