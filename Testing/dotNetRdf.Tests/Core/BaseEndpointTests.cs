using System;
using System.Net;
using FluentAssertions;
using Xunit;

namespace VDS.RDF.Core;

[Obsolete("Tests for obsolete class")]
public class BaseEndpointTests
{
    [Fact]
    public void ConstructorSetsTargetUri()
    {
        var target = new Uri("http://example.org/");
        var endpoint = new TestEndpoint(target);
        var request = endpoint.CreateWebRequest();
        request.RequestUri.Should().Be(target);
    }

    [Fact]
    public void ApplyRequestOptionsSetsUserAgentHeader()
    {
        var ep = new TestEndpoint(new Uri("http://example.org/")) {UserAgent = "My User Agent String"};
        var req = ep.CreateWebRequest();
        req.Headers["User-Agent"].Should().Be("My User Agent String");
    }

    [Fact]
    public void ApplyRequestOptionsSetsCredentials()
    {
        var target = new Uri("http://example.org/");
        var cred = new NetworkCredential("test", "password");
        var endpoint = new TestEndpoint(target){Credentials = cred};
        var request = endpoint.CreateWebRequest();
        request.Credentials.Should().Be(cred);
    }


    [Fact]
    public void ApplyRequestOptionsSetsWebProxy()
    {
        var target = new Uri("http://example.org/");
        var proxyUri = new Uri("http://proxy.contoso.com/");
        var proxy = new WebProxy("http://proxy.contoso.com/");
        var endpoint = new TestEndpoint(target) { Proxy = proxy};
        var request = endpoint.CreateWebRequest();
        request.Proxy.Should().Be(proxy);
        request.Proxy.GetProxy(target).Should().Be(proxyUri);
    }

    [Fact]
    public void ApplyRequestOptionsWithProxyAndTargetCredentials()
    {
        var target = new Uri("http://example.org/");
        var proxyUri = new Uri("http://proxy.contoso.com/");
        var cred = new NetworkCredential("test", "password");
        var proxy = new WebProxy(proxyUri);
        var endpoint = new TestEndpoint(target) { Proxy = proxy, Credentials = cred };
        var request = endpoint.CreateWebRequest();
        request.Proxy.GetProxy(target).Should().Be(proxyUri);
        request.Proxy.Credentials.Should().BeNull();
        request.Credentials.Should().Be(cred);
    }

    [Fact]
    public void ApplyRequestOptionsSetsWebProxyCredentials()
    {
        var target = new Uri("http://example.org/");
        var proxyUri = new Uri("http://proxy.contoso.com/");
        var cred = new NetworkCredential("test", "password");
        var proxy = new WebProxy(proxyUri);
        var endpoint = new TestEndpoint(target) { Proxy = proxy, Credentials = cred, UseCredentialsForProxy = true};
        var request = endpoint.CreateWebRequest();
        request.Proxy.GetProxy(target).Should().Be(proxyUri);
        request.Proxy.Credentials.Should().Be(cred);
    }

    [Fact]
    public void ApplyRequestOptionsDisablesKeepAlive()
    {
        var target = new Uri("http://example.org/");
        var endpoint = new TestEndpoint(target);
        var request = endpoint.CreateWebRequest();
        request.KeepAlive.Should().BeFalse();
    }

    [Fact]
    public void ApplyRequestOptionsSetsTimeout()
    {
        var target = new Uri("http://example.org/");
        var endpoint = new TestEndpoint(target) {Timeout = 1234};
        var request = endpoint.CreateWebRequest();
        request.Timeout.Should().Be(1234);
    }
}

[Obsolete]
class TestEndpoint : BaseEndpoint
{
    public TestEndpoint(Uri target) : base(target) { }

    public HttpWebRequest CreateWebRequest()
    {
        var req = (HttpWebRequest) WebRequest.Create(Uri);
        ApplyRequestOptions(req);
        return req;
    }
}
