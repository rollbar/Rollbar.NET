#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.NetPlatformExtensions
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using global::Rollbar.NetPlatformExtensions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;


    [TestClass]
    [TestCategory(nameof(RollbarLoggerFixture))]
    public class RollbarLoggerFixture
        : RollbarLiveFixtureBase
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
        public void TestBasics()
        {

        }
    }
}
