# Configuring Permissions 

Permissions are configured very simply as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:permission a dnr:Permission ;
  dnr:type "VDS.RDF.Configuration.Permissions.Permission" ;
  dnr:action "ACTION" .
```

Or you can specify permission sets which specify that allow multiple actions to bee grouped together:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:permission a dnr:Permission ;
  dnr:type "VDS.RDF.Configuration.Permissions.PermissionSet" ;
  dnr:action "ACTION" ;
  dnr:action "OTHER" .
```

For permission sets multiple `dnr:action` properties may be specified.