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
    [TestCategory(nameof(RollbarScopeFixture))]
    public class RollbarScopeFixture
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
            var scopes = new RollbarScope[] {
                    new RollbarScope(@"one", 1),
                    new RollbarScope(@"two", 2),
                    new RollbarScope(@"three", 3),
                    new RollbarScope(@"four", 4),
                };

            Assert.IsNull(RollbarScope.Current);
            
            // push in all the scopes:
            List<IDisposable> disposablePopActions = new List<IDisposable>(scopes.Length);
            int i = 0;
            while (i < scopes.Length)
            {
                disposablePopActions.Add(RollbarScope.Push(scopes[i]));

                Assert.AreSame(scopes[i], RollbarScope.Current);
                if (i > 0)
                {
                    Assert.AreSame(scopes[i - 1], RollbarScope.Current.Next);
                }
                else
                {
                    Assert.IsNull(RollbarScope.Current.Next);
                }

                i++;
            }

            // pop out all the scopes (on action disposal):
            while(--i >= 0)
            {
                disposablePopActions[i].Dispose();
                
                if (i > 0)
                {
                    Assert.AreSame(scopes[i - 1], RollbarScope.Current);
                }
                else
                {
                    Assert.IsNull(RollbarScope.Current);
                }
            }

            // no scope is expected:
            Assert.IsNull(RollbarScope.Current);
        }
    }
}
