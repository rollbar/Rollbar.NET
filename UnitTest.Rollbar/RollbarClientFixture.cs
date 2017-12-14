#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    [TestClass]
    [TestCategory("RollbarClientFixture")]
    public class RollbarClientFixture
    {

        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        //There is nothing really to unit test regarding the RollbarClient type.
        //It does not have any processing logic at all and just makes a couple of 
        //very straightforward into .NET type and Newtonsoft.Json SDKs.
        
    }
}
