using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Application;
using static Facebook.Workrooms.Application.FacebookUri;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_prodinfra)]
  public class FacebookURITest {
    private const string API_TIER = "some_tier";

    [Test]
    public void TestGetURI() {
      var provider = new FacebookUriProvider(new ConfigurationOptions());
      var providerWithCustomApiTier = new FacebookUriProvider(
        new ConfigurationOptions() {
          apiTier = API_TIER,
        }
      );

      Assert.AreEqual(
        provider.GetUri(
            new UriConfig() {
              Subdomain = Subdomain.API,
              Host = Host.FACEBOOK
            }
          )
          .ToUrl(),
        "https://api.facebook.com"
      );
      Assert.AreEqual(
        provider.GetUri(
            new UriConfig() {
              Subdomain = Subdomain.GRAPH,
              Host = Host.OCULUS
            }
          )
          .ToUrl(),
        "https://graph.oculus.com"
      );
      Assert.AreEqual(
        provider.GetUri(
            new UriConfig() {
              Subdomain = Subdomain.GRAPH,
              Host = Host.FACEBOOK,
              Endpoint = Endpoint.GRAPHQL
            }
          )
          .ToUrl(),
        "https://graph.facebook.com/graphql"
      );
      Assert.AreEqual(
        providerWithCustomApiTier.GetUri(
            new UriConfig() {
              Subdomain = Subdomain.GRAPH,
              Host = Host.FACEBOOK,
              Endpoint = Endpoint.GRAPHQL,
              QueryString = "testProperQueryString"
            }
          )
          .ToUrl(),
        $"https://graph.{API_TIER}.facebook.com/graphql?testProperQueryString"
      );

      Assert.AreEqual(
        providerWithCustomApiTier.GetUri(
            new UriConfig() {
              Subdomain = Subdomain.GRAPH,
              Host = Host.OCULUS,
              RawQueryString = "someRawQueryString/does?stuff&like=this"
            }
          )
          .ToUrl(),
        $"https://graph.{API_TIER}.oculus.com/someRawQueryString/does?stuff&like=this"
      );
      Assert.AreEqual(
        providerWithCustomApiTier.GetUri(
            new UriConfig() {
              Subdomain = Subdomain.GRAPH,
              Host = Host.OCULUS,
              Endpoint = Endpoint.NONE,
              QueryString = "someQueryString"
            }
          )
          .ToUrlWithoutProtocol(),
        $"graph.{API_TIER}.oculus.com/?someQueryString"
      );

      var uriConfigGraphFacebook1337 = new UriConfig() {
        Subdomain = Subdomain.GRAPH,
        Host = Host.FACEBOOK,
        Port = 1337
      };
      Assert.AreEqual(provider.GetUri(uriConfigGraphFacebook1337).ToUrl(), "https://graph.facebook.com:1337");
      Assert.AreEqual(
        providerWithCustomApiTier.GetUri(uriConfigGraphFacebook1337).ToUrl(),
        $"https://graph.{API_TIER}.facebook.com:1337"
      );

      var uriConfigLocalhost9999 = new UriConfig() {
        Protocol = Protocol.HTTP,
        Host = Host.LOCALHOST,
        Port = 9999
      };
      Assert.AreEqual(provider.GetUri(uriConfigLocalhost9999).ToUrl(), "http://localhost:9999");
      // custom tier doesn't matter for localhost
      Assert.AreEqual(providerWithCustomApiTier.GetUri(uriConfigLocalhost9999).ToUrl(), $"http://localhost:9999");
    }
  }
}
