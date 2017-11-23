namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory("RollbarLocatorFixture")]
    public class RollbarLocatorFixture
    {
        private IRollbar _logger = null;

        [TestInitialize]
        public void SetupFixture()
        {
            RollbarConfig loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
            _logger = RollbarFactory.CreateNew().Configure(loggerConfig);
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        public void LocatesTheSameInstance()
        {
            Assert.AreEqual(RollbarLocator.RollbarInstance, RollbarLocator.RollbarInstance);
        }

        [TestMethod]
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
