**** DISCLAIMER ****
This hook was developed and tested against the Mercurial 2.3.1 Python 2.7 package (x86) on an IIS 7.5
web server configured using the following tutorials as a reference:
http://www.jeremyskinner.co.uk/mercurial-on-iis7/
http://stackingcode.com/blog/2011/02/24/running-a-mercurial-server-on-iis-7-5-windows-server-2008-r2
http://trueclarity.wordpress.com/2012/06/06/setting-up-mercurial-2-2-on-iis7/



The following describes the configuration of the Mercurial ChangeGroup Hook

Configuring your repositories
For each repository you want the hook to run you will need to copy the following lines to the file
.hg\hgrc in the repository.

* Change the PATH_TO_EXE to the location of the hooks executable.
* Change the localhost to the url/domain of your BugNet site.

++++++++ COPY BELOW THIS LINE ++++++++

[hooks]
changegroup.MercurialChangeGroupHook = "PATH_TO_EXE\BugNET.MercurialChangeGroupHook.exe"

[tortoisehg]
issue.regex = \[?([A-Za-z]{1,50}-(\d+))\]?
issue.link = http://localhost/Issues/IssueDetail.aspx?id={2}

++++++++ COPY ABOVE THIS LINE ++++++++


Configuring the Hook
Configure the BugNET.MercurialChangeGroupHook.exe.config appSettings properties for your environment.

BugNetUsername
- When not using Windows Authentication set this to a BugNET username that has permissions to logon to BugNET.

BugNetPassword
- When not using Windows Authentication set this to the password for the BugNetUsername entered.

BugNetServicesUrl
This is the fully qualified Url to the web services, This would be the same Url as in the Application
Settings - Basic - Default Url along with WebServices/BugNetServices.asmx as the path part.  i.e.
http://localhost/WebServices/BugNetServices.asmx

BugNetWindowsAuthentication
True when using Windows Authentication for BugNET, otherwise False.

Configuration of logging
By default the Hook logs to a local file in the same folder as the exe.  However this can be changed to log 
to BugNET by doing the following.

Copy the <log4net> section from your BugNET log4net.config file over the <log4net> section in the
BugNET.MercurialChangeGroupHook.exe.config file.  Depending on your environment you may have to configure the
connection string to use a username/password.  The reason for this is the user account who commits must have access to 
the BugNET database as well.


*** Note about Windows Authentication ***
Depending on how your environment is configured some things to note.

*	When a user synchronizes with your remote repository the users user/password credential is used when the 
	hook is executed which may be passed on to the BugNET Web service and logging (when integrated auth is used).  
	This could be an issue if your environment is not under Active Directory (standalone servers) as the 
	username/passwords will need to be synched across the servers.

	i.e.
	- File server hosts source (either file system or http/https)
	- Web server hosts BugNET
	- Database server hosts BugNET Db

	User Commits to central repo (File server) -> 
		hook is executed (File server) -> 
			Web service called (Web server) -> Save revision called (Database server)
			Logging called (Database server)

In a windows auth environment the users credentials will be passed along through all servers.