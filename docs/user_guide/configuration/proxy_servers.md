# Configuring Proxy Servers 

Proxy Servers are configured very simply as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proxy a dnr:Proxy ;
  dnr:server "http://proxy.example.com" ;
  dnr:user "username" ;
  dnr:password "password" .
```

Username and password are optional for proxies. Note also that no `dnr:type` property is required.