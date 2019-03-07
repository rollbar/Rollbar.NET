#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Specialized;
    using System.Net.Http;
    using System.Security.Principal;

#if NETFX
    using System.ServiceModel.Channels;
    using System.Web;
    using System.Web.SessionState;
#endif

    [TestClass]
    [TestCategory(nameof(HttpRequestMessagePackageDecoratorFixture))]
    public class HttpRequestMessagePackageDecoratorFixture
    {

        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

#if NETFX

        [TestMethod]
        public void RequestUserIPPolicyTest()
        {
            var testCases = new[]
            {
                //(policy: IpAddressCollectionPolicy.Collect, initialUserHost: "10.9.8.7", expectedUserHost: "10.9.8.7"),

                //IPv4:
                new { Policy = IpAddressCollectionPolicy.Collect, InitialUserHost = @"10.9.8.7", ExpectedUserHost = @"10.9.8.7" },
                new { Policy = IpAddressCollectionPolicy.CollectAnonymized, InitialUserHost = @"10.9.8.7", ExpectedUserHost = @"10.9.8.0/24" },
                new { Policy = IpAddressCollectionPolicy.DoNotCollect, InitialUserHost = @"10.9.8.7", ExpectedUserHost = null as string },
                //host name:
                new { Policy = IpAddressCollectionPolicy.CollectAnonymized, InitialUserHost = @"UserHost.com", ExpectedUserHost = @"UserHost.com" },
                //IPv6:
                new { Policy = IpAddressCollectionPolicy.Collect, InitialUserHost = @"2001:0db8:0000:0042:0000:8a2e:0370:7334", ExpectedUserHost = @"2001:0db8:0000:0042:0000:8a2e:0370:7334" },
                new { Policy = IpAddressCollectionPolicy.CollectAnonymized, InitialUserHost = @"2001:0db8:0000:0042:0000:8a2e:0370:7334", ExpectedUserHost = @"2001:0db8:00..." },
                new { Policy = IpAddressCollectionPolicy.DoNotCollect, InitialUserHost = @"2001:0db8:0000:0042:0000:8a2e:0370:7334", ExpectedUserHost = null as string },
            };

            foreach(var testCase in testCases)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Testing: policy = {testCase.Policy}, initialUserHost = {testCase.InitialUserHost}, expectedUserHost = {testCase.ExpectedUserHost}"
                    );
                RequestUserIPPolicyTest(testCase.Policy, testCase.InitialUserHost, testCase.ExpectedUserHost);
            }
        }

        private static void RequestUserIPPolicyTest(
            IpAddressCollectionPolicy policy
            , string initialUserHost
            , string expectedUserHost
            )
        {
            IRollbarPackage package = new MessagePackage("Some message");
            
            var rollbarConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken);
            rollbarConfig.IpAddressCollectionPolicy = policy;
            var msg = FakeHttpRequestMessage(initialUserHost);
            package = new HttpRequestMessagePackageDecorator(package, msg, rollbarConfig);

            var request = package.PackageAsRollbarData().Request;

            Assert.AreEqual(expectedUserHost, request.UserIp);
        }

        private static HttpRequestMessage FakeHttpRequestMessage(string userHost)
        {
            var msg = new Mock<HttpRequestMessage>();
            HttpContextBase ctx = 
                FakeHttpContext(@"http://www.url.com", userHost, @"http://www.referrer.com");
            msg.Object.Properties.Add("MS_HttpContext", ctx);
            return msg.Object;
        }

        public static HttpContextBase FakeHttpContext(string url, string ip, string referrer)
        {
            Uri uri = new Uri(url);

            var context = new Mock<HttpContextBase>();
            var files = new Mock<HttpFileCollectionBase>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new Mock<HttpSessionStateBase>();
            var server = new Mock<HttpServerUtilityBase>();

            var user = new Mock<IPrincipal>();
            var identity = new Mock<IIdentity>();

            request.Setup(req => req.ApplicationPath).Returns("~/");
            request.Setup(req => req.AppRelativeCurrentExecutionFilePath).Returns("~/");
            request.Setup(req => req.PathInfo).Returns(string.Empty);
            request.Setup(req => req.Form).Returns(new NameValueCollection());
            request.Setup(req => req.QueryString).Returns(HttpUtility.ParseQueryString(uri.Query));
            request.Setup(req => req.Files).Returns(files.Object);
            request.Setup(req => req.UserHostAddress).Returns(ip);
            request.Setup(req => req.UrlReferrer).Returns(new Uri(referrer));
            request.Setup(req => req.Url).Returns(uri);
            request.Setup(req => req.RawUrl).Returns(url);

            response.Setup(res => res.ApplyAppPathModifier(It.IsAny<string>())).Returns((string virtualPath) => virtualPath);

            user.Setup(usr => usr.Identity).Returns(identity.Object);

            identity.SetupGet(ident => ident.IsAuthenticated).Returns(true);

            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session.Object);
            context.Setup(ctx => ctx.Server).Returns(server.Object);
            context.Setup(ctx => ctx.User).Returns(user.Object);

            return context.Object;
        }

#endif

    }
}
