# Configuring User Groups 

User Groups are configured very simply as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:group a dnr:UserGroup ;
  dnr:type "VDS.RDF.Configuration.Permissions.UserGroup" ;
  dnr:member _:user ;
  dnr:allow _:perm1 ;
  dnr:deny _:perm2 ;
  dnr:requiresAuthentication false ;
  dnr:permissionModel "AllowDeny" .

_:user a dnr:User ;
  dnr:user "username" ;
  dnr:password "password" .

_:perm1 a dnr:Permission ;
  dnr:type "VDS.RDF.Configuration.Permissions.Permission" ;
  dnr:action "INSERT" .

_:perm2 a dnr:Permission ;
  dnr:type "VDS.RDF.Configuration.Permissions.PermissionSet" ;
  dnr:action "DROP" ;
  dnr:action "DELETE" ;
  dnr:action "MODIFY" ;
  dnr:action "DELETE DATA" .
```

In the above example we specify a user group with a single member, members are specified via the `dnr:member` property. Permissions are granted to the group using the `dnr:allow` and `dnr:deny` properties - see [Configuration API - Permissions](permissions.md) for more details.

The `dnr:requiresAuthentication` property specifies whether the permissions apply only to authenticated users or to unauthenticated users i.e. guests. Setting to to false means that the permissions apply to guests.

The `dnr:permissionModel` sets the permission model of the group to one of the supported models as specified by the [`PermissionModel`](xref:VDS.RDF.Configuration.Permissions.PermissionModel) enumeration.