dotNetRDF for NuGet
===================

For help and support with dotNetRDF please see the Support page at our website:

http://www.dotnetrdf.org/content.asp?pageID=Support

Windows Phone/Silverlight Issue
-------------------------------

Developers installing via NuGet should be aware of the following issue when using the Silverlight and Windows Phone builds of the library.

Due to an upstream issue with HtmlAgilityPack projects for Silverlight and Windows Phone must reference System.Xml.XPath in order to
compile correctly (http://htmlagilitypack.codeplex.com/workitem/32678)

For Silverlight projects this should just be a case of adding the following to your project file either via VS or by hand in the appropriate section
of the project file:

<Reference Include="System.Xml.XPath" />

For Windows Phone 7 projects you will need to add the following since this DLL is not part of the official Windows Phone SDK but we are assured by the
HAP developers that it is fully compatible with Windows Phone:

    <Reference Include="System.Xml.XPath">
      <HintPath>$(ProgramFiles(x86))\Microsoft SDKs\Silverlight\v4.0\Libraries\Client\System.Xml.XPath.dll</HintPath>
    </Reference>

Note if you add this DLL to a Windows Phone project via VS you will receive a warning message, we suggest you add this manually to Windows Phone
project files so you can use the environment variable and make your project file portable since adding via VS will use a relative path based on
your project file location
