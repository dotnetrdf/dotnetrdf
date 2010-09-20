using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqToRdf;
using System.Data.Linq;
using System;

namespace RdfMetal.Foaf
{
[assembly: Ontology(
    BaseUri = "http://xmlns.com/foaf/0.1/",
    Name = "foaf",
    Prefix = "foaf",
    UrlOfOntology = "http://xmlns.com/foaf/0.1/")]


    public partial class foafDataContext : RdfDataContext
    {
        public foafDataContext(TripleStore store) : base(store)
        {
        }
        public foafDataContext(string store) : base(new TripleStore(store))
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

[OwlResource(OntologyName="foaf", RelativeUriReference="Person")]
public partial class Person : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "surname")]
  public string surname {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "family_name")]
  public string family_name {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "geekcode")]
  public string geekcode {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "firstName")]
  public string firstName {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "plan")]
  public string plan {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "myersBriggs")]
  public string myersBriggs {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "topic_interest")]
  public LinqToRdf.OwlInstanceSupertype topic_interest {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "currentProject")]
  public LinqToRdf.OwlInstanceSupertype currentProject {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "pastProject")]
  public LinqToRdf.OwlInstanceSupertype pastProject {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "foaf", RelativeUriReference = "knows")]
public string knowsUri { get; set; }

private EntityRef<Person> _knows { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "knows")]
public Person knows
{
    get
    {
        if (_knows.HasLoadedOrAssignedValue)
            return _knows.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _knows = new EntityRef<Person>(from x in ctx.Persons where x.HasInstanceUri(knowsUri) select x);
            return _knows.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "foaf", RelativeUriReference = "img")]
public string imgUri { get; set; }

private EntityRef<Image> _img { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "img")]
public Image img
{
    get
    {
        if (_img.HasLoadedOrAssignedValue)
            return _img.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _img = new EntityRef<Image>(from x in ctx.Images where x.HasInstanceUri(imgUri) select x);
            return _img.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "foaf", RelativeUriReference = "workplaceHomepage")]
public string workplaceHomepageUri { get; set; }

private EntityRef<Document> _workplaceHomepage { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "workplaceHomepage")]
public Document workplaceHomepage
{
    get
    {
        if (_workplaceHomepage.HasLoadedOrAssignedValue)
            return _workplaceHomepage.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _workplaceHomepage = new EntityRef<Document>(from x in ctx.Documents where x.HasInstanceUri(workplaceHomepageUri) select x);
            return _workplaceHomepage.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "foaf", RelativeUriReference = "workInfoHomepage")]
public string workInfoHomepageUri { get; set; }

private EntityRef<Document> _workInfoHomepage { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "workInfoHomepage")]
public Document workInfoHomepage
{
    get
    {
        if (_workInfoHomepage.HasLoadedOrAssignedValue)
            return _workInfoHomepage.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _workInfoHomepage = new EntityRef<Document>(from x in ctx.Documents where x.HasInstanceUri(workInfoHomepageUri) select x);
            return _workInfoHomepage.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "foaf", RelativeUriReference = "schoolHomepage")]
public string schoolHomepageUri { get; set; }

private EntityRef<Document> _schoolHomepage { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "schoolHomepage")]
public Document schoolHomepage
{
    get
    {
        if (_schoolHomepage.HasLoadedOrAssignedValue)
            return _schoolHomepage.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _schoolHomepage = new EntityRef<Document>(from x in ctx.Documents where x.HasInstanceUri(schoolHomepageUri) select x);
            return _schoolHomepage.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "foaf", RelativeUriReference = "interest")]
public string interestUri { get; set; }

private EntityRef<Document> _interest { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "interest")]
public Document interest
{
    get
    {
        if (_interest.HasLoadedOrAssignedValue)
            return _interest.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _interest = new EntityRef<Document>(from x in ctx.Documents where x.HasInstanceUri(interestUri) select x);
            return _interest.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "foaf", RelativeUriReference = "publications")]
public string publicationsUri { get; set; }

private EntityRef<Document> _publications { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "publications")]
public Document publications
{
    get
    {
        if (_publications.HasLoadedOrAssignedValue)
            return _publications.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _publications = new EntityRef<Document>(from x in ctx.Documents where x.HasInstanceUri(publicationsUri) select x);
            return _publications.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="Document")]
public partial class Document : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "primaryTopic")]
  public LinqToRdf.OwlInstanceSupertype primaryTopic {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "topic")]
  public LinqToRdf.OwlInstanceSupertype topic {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="Agent")]
public partial class Agent : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "mbox_sha1sum")]
  public string mbox_sha1sum {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "gender")]
  public string gender {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "jabberID")]
  public string jabberID {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "aimChatID")]
  public string aimChatID {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "icqChatID")]
  public string icqChatID {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "yahooChatID")]
  public string yahooChatID {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "msnChatID")]
  public string msnChatID {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "birthday")]
  public string birthday {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "mbox")]
  public LinqToRdf.OwlInstanceSupertype mbox {get;set;} // 
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "made")]
  public LinqToRdf.OwlInstanceSupertype made {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "foaf", RelativeUriReference = "weblog")]
public string weblogUri { get; set; }

private EntityRef<Document> _weblog { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "weblog")]
public Document weblog
{
    get
    {
        if (_weblog.HasLoadedOrAssignedValue)
            return _weblog.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _weblog = new EntityRef<Document>(from x in ctx.Documents where x.HasInstanceUri(weblogUri) select x);
            return _weblog.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "foaf", RelativeUriReference = "openid")]
public string openidUri { get; set; }

private EntityRef<Document> _openid { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "openid")]
public Document openid
{
    get
    {
        if (_openid.HasLoadedOrAssignedValue)
            return _openid.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _openid = new EntityRef<Document>(from x in ctx.Documents where x.HasInstanceUri(openidUri) select x);
            return _openid.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "foaf", RelativeUriReference = "tipjar")]
public string tipjarUri { get; set; }

private EntityRef<Document> _tipjar { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "tipjar")]
public Document tipjar
{
    get
    {
        if (_tipjar.HasLoadedOrAssignedValue)
            return _tipjar.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _tipjar = new EntityRef<Document>(from x in ctx.Documents where x.HasInstanceUri(tipjarUri) select x);
            return _tipjar.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "foaf", RelativeUriReference = "holdsAccount")]
public string holdsAccountUri { get; set; }

private EntityRef<OnlineAccount> _holdsAccount { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "holdsAccount")]
public OnlineAccount holdsAccount
{
    get
    {
        if (_holdsAccount.HasLoadedOrAssignedValue)
            return _holdsAccount.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _holdsAccount = new EntityRef<OnlineAccount>(from x in ctx.OnlineAccounts where x.HasInstanceUri(holdsAccountUri) select x);
            return _holdsAccount.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="Organization")]
public partial class Organization : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="Project")]
public partial class Project : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="Group")]
public partial class Group : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "foaf", RelativeUriReference = "member")]
public string memberUri { get; set; }

private EntityRef<Agent> _member { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "member")]
public Agent member
{
    get
    {
        if (_member.HasLoadedOrAssignedValue)
            return _member.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _member = new EntityRef<Agent>(from x in ctx.Agents where x.HasInstanceUri(memberUri) select x);
            return _member.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="Image")]
public partial class Image : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "depicts")]
  public LinqToRdf.OwlInstanceSupertype depicts {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "foaf", RelativeUriReference = "thumbnail")]
public string thumbnailUri { get; set; }

private EntityRef<Image> _thumbnail { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "thumbnail")]
public Image thumbnail
{
    get
    {
        if (_thumbnail.HasLoadedOrAssignedValue)
            return _thumbnail.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _thumbnail = new EntityRef<Image>(from x in ctx.Images where x.HasInstanceUri(thumbnailUri) select x);
            return _thumbnail.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="PersonalProfileDocument")]
public partial class PersonalProfileDocument : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="OnlineAccount")]
public partial class OnlineAccount : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "foaf", RelativeUriReference = "accountName")]
  public string accountName {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "foaf", RelativeUriReference = "accountServiceHomepage")]
public string accountServiceHomepageUri { get; set; }

private EntityRef<Document> _accountServiceHomepage { get; set; }

[OwlResource(OntologyName = "foaf", RelativeUriReference = "accountServiceHomepage")]
public Document accountServiceHomepage
{
    get
    {
        if (_accountServiceHomepage.HasLoadedOrAssignedValue)
            return _accountServiceHomepage.Entity;
        if (DataContext != null)
        {
            var ctx = (foafDataContext)DataContext;
            _accountServiceHomepage = new EntityRef<Document>(from x in ctx.Documents where x.HasInstanceUri(accountServiceHomepageUri) select x);
            return _accountServiceHomepage.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="OnlineGamingAccount")]
public partial class OnlineGamingAccount : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="OnlineEcommerceAccount")]
public partial class OnlineEcommerceAccount : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="foaf", RelativeUriReference="OnlineChatAccount")]
public partial class OnlineChatAccount : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}



}