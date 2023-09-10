# dotNetRDF Assembly Signing

Prior to the 3.0 release, all of the dotNetRDF assemblies were fully signed using a key which is kept in the dotNetRDF source repository.
Although this can be convenient for using NuGet packages directly in contexts where full trust is required, it is also a potential security risk because the private key that we use for the signing is available to anyone, including bad actors.
To that end we decided in the 3.0 release to completely cease signing the assemblies that we distribute.
However following feedback from our users and following guidance from both the NuGet and from the MS documentation, in 3.0.1 we reinstated assembly signing, but opted to use the [`PublicSign`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/security#publicsign) option for signing the assemblies.

The `PublicSign` option means that our public key is included in the assembly and a flag is set on the assembly indicating that it is "signed", but it does not actually cryptographically sign the assembly.
This approach addresses the primary concern that was raised by users when we stopped signing assemblies - a `PublicSign` signed assembly is still considered to be "strong-named" and so can be referenced by code that is building a strong-named assembly output.
However there are some limitations to this approach (as documented [here](https://github.com/dotnet/runtime/blob/main/docs/project/public-signing.md)):

> * You will not be able to install the assembly to the Global Assembly Cache (GAC)
> * You will not be able to load the assembly in an AppDomain where shadow copying is turned on.
> * You will not be able to load the assembly in a partially trusted AppDomain
> * You will not be able to pre-compile ASP.NET applications

In all of these cases we believe that rather than trust an assembly that has been signed with a (deliberately!) compromised key the safest approach would be to use the source code to rebuild the assembly for yourself and sign with your own key.
If you want to trust the binaries from NuGet you can also choose to just directly sign the assembly yourself:

```
sn -R dotNetRdf.dll c:\path\to\your\key.snk
```

If you are building applications that require our assemblies to be deployed to your (or your users') Global Assembly Cache we *strongly* recommend that you follow the approach of building dotNetRDF from source and signing the assemblies with your own key.

> [!NOTE]
> Precompilation in ASP.NET MVC can be disabled with the build property ` <MvcBuildViews>false</MvcBuildViews>`.
> Shadow-copying in ASP.NET applications can be disabled by adding `<hostingEnvironment shadowCopyBinAssemblies="false" />` under the `system.web` tag in the application's `Web.config` file