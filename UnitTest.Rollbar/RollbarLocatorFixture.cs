#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Infrastructure;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory(nameof(RollbarLocatorFixture))]
    public class RollbarLocatorFixture
    {

        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        public static void ProducesValidInstance()
        {
            var rollbarLogger = RollbarLocator.RollbarInstance as RollbarLogger;
            Assert.IsNotNull(rollbarLogger);
        }

        [TestMethod]
        public void LocatesTheSameInstance()
        {
            Assert.AreSame(RollbarLocator.RollbarInstance, RollbarLocator.RollbarInstance);
        }

        [TestMethod]
        [Timeout(10000)]
        public void LocatesTheSameInstanceInMultithreadedEnvironment()
        {
            const int maxIterations = 100;
            const int maxTasks = 100;

            byte[] delays = new byte[maxTasks];
            var randomGenerator = new Random();
            int iteration = 0;
            while (iteration++ < maxIterations)
            {
                randomGenerator.NextBytes(delays);

                List<Task<IRollbar>> tasks = new List<Task<IRollbar>>(maxTasks);
                int i = 0;
                while (i < maxTasks)
                {
                    Trace.WriteLine("Task #" + i);
                    tasks.Add(

                        Task.Factory.StartNew<IRollbar>((object state) =>
                        {
                            Trace.WriteLine("Starting thread: " + Thread.CurrentThread.ManagedThreadId);
                            byte delay = (byte)state;
                            Trace.WriteLine("Delay" + delay);
                            Thread.Sleep(TimeSpan.FromTicks(delay));
                            var rollbar = RollbarLocator.RollbarInstance;
                            Trace.WriteLine("Ending thread: " + Thread.CurrentThread.ManagedThreadId);
                            return rollbar;
                        }
                        , delays[i]
                        )

                    );
                    i++;
                }

                Task.WaitAll(tasks.ToArray());
                foreach(var task in tasks)
                {
                    Assert.AreEqual(tasks.First().Result, task.Result);
                }

            }

        }

    }
}
