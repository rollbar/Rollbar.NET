#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.Net.AspNet.WebApi
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Specialized;
    using System.Security.Principal;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http.Controllers;
    using global::Rollbar.Net.AspNet.WebApi;

    [TestClass]
    [TestCategory(nameof(HttpActionContextPackageDecoratorFixture))]
    public class HttpActionContextPackageDecoratorFixture
    {

        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void TestHttpActionContextPackageDecorator()
        {
            var httpActionContext= FakeHttpActionContext();
            var package = FakePackege();
            package = new HttpActionContextPackageDecorator(package, httpActionContext);
            try
            {
                _ = package.RollbarData;
            }
            catch(System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                Assert.Fail("No exception expected");
            }
        }

        public static IRollbarPackage FakePackege()
        {
            var package = new ExceptionPackage(new NotImplementedException("Fake exception"));
            return package;
        }

        public static HttpActionContext FakeHttpActionContext()//(string url, string ip, string referrer)
        {
            var context = new HttpActionContext();
            return context;
        }

    }
}
