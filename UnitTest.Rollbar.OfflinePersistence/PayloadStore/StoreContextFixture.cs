#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.PayloadStore 
{
    using global::Rollbar.Telemetry;
    using dto = global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Rollbar.PayloadStore;
    using Microsoft.EntityFrameworkCore;

    [TestClass]
    [TestCategory(nameof(StoreContextFixture))]
    public class StoreContextFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
            //SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            ResetStoreData();
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        private void ResetStoreData()
        {
            using (StoreContext storeContext = new StoreContext())
            {
                var payloadRecords = storeContext.PayloadRecords.ToArray();
                var destinations = storeContext.Destinations.ToArray();

                storeContext.RemoveRange(payloadRecords);
                storeContext.RemoveRange(destinations);

                storeContext.SaveChanges();
            }

            using (StoreContext storeContext = new StoreContext())
            {
                Assert.AreEqual(0, storeContext.PayloadRecords.Count());
                Assert.AreEqual(0, storeContext.Destinations.Count());
            }

        }

        [TestMethod]
        public void BasicTest()
        {
            using (StoreContext storeContext = new StoreContext())
            {
                Assert.AreEqual(0, storeContext.PayloadRecords.Count());
                Assert.AreEqual(0, storeContext.Destinations.Count());

                Destination destination = new Destination()
                {
                    AccessToken = "token1", 
                    Endpoint = "http://endpoint.com"
                };
                Assert.AreNotEqual(Guid.Empty, destination.ID);

                destination.PayloadRecords.Add(new PayloadRecord() {PayloadJson = "payload1", Timestamp = DateTime.UtcNow, });

                storeContext.Add(destination);
                storeContext.SaveChanges();
            }

            using (StoreContext storeContext = new StoreContext())
            {
                Assert.AreEqual(1, storeContext.Destinations.Count());
                Assert.AreEqual(1, storeContext.PayloadRecords.Count());
            }

            using (StoreContext storeContext = new StoreContext())
            {
                Destination destination = storeContext.Destinations.FirstOrDefault(d => d.AccessToken == "token1");
                Assert.IsNotNull(destination);
                Assert.AreEqual("token1", destination.AccessToken);

                PayloadRecord payloadRecord = null;

                payloadRecord = new PayloadRecord() {PayloadJson = "payload2", Timestamp = DateTime.UtcNow, };
                storeContext.Entry(payloadRecord).State = EntityState.Added;
                destination.PayloadRecords.Add(payloadRecord);
                payloadRecord = new PayloadRecord() {PayloadJson = "payload3", Timestamp = DateTime.UtcNow, Destination = destination};
                destination.PayloadRecords.Add(payloadRecord);
                storeContext.Add(payloadRecord);
                payloadRecord = new PayloadRecord() {PayloadJson = "payload4", Timestamp = DateTime.UtcNow, Destination = destination};
                destination.PayloadRecords.Add(payloadRecord);
                storeContext.PayloadRecords.Add(payloadRecord);

                destination = new Destination()
                {
                    AccessToken = "token2", 
                    Endpoint = "http://endpoint.com"
                };
                Assert.AreNotEqual(Guid.Empty, destination.ID);
                storeContext.Entry(destination).State = EntityState.Added;

                payloadRecord = new PayloadRecord() {PayloadJson = "payload5", Timestamp = DateTime.UtcNow, };
                storeContext.Entry(payloadRecord).State = EntityState.Added;
                destination.PayloadRecords.Add(payloadRecord);

                payloadRecord = new PayloadRecord() {PayloadJson = "payload6", Timestamp = DateTime.UtcNow, Destination = destination};
                storeContext.Entry(payloadRecord).State = EntityState.Added;
                storeContext.Add(payloadRecord);

                payloadRecord = new PayloadRecord() {PayloadJson = "payload7", Timestamp = DateTime.UtcNow, Destination = destination};
                storeContext.Entry(payloadRecord).State = EntityState.Added;
                storeContext.PayloadRecords.Add(payloadRecord);

                storeContext.Add(destination);
                storeContext.SaveChanges();
            }

            using (StoreContext storeContext = new StoreContext())
            {
                Assert.AreEqual(2, storeContext.Destinations.Count());
                Assert.AreEqual(7, storeContext.PayloadRecords.Count());

                Assert.AreEqual(4, 
                    storeContext.Destinations
                        .Where(d => d.AccessToken == "token1")
                        .Include(d => d.PayloadRecords)
                        .First()
                        .PayloadRecords
                        .Count()
                    );
                Assert.AreEqual(3, 
                    storeContext.Destinations
                        .Where(d => d.AccessToken == "token2")
                        .Include(d => d.PayloadRecords)
                        .First()
                        .PayloadRecords
                        .Count()
                    );
            }
        }

        [TestMethod]
        public void FixtureSetupTest()
        {
            using (StoreContext storeContext = new StoreContext())
            {
                Assert.AreEqual(0, storeContext.PayloadRecords.Count());
                Assert.AreEqual(0, storeContext.Destinations.Count());
            }
        }
    }
}
