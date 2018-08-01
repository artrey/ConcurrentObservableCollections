using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentObservableCollections.Tests
{
    [TestClass]
    public class ConcurrentObservableDictionaryTest
    {
        [TestMethod]
        public void TestEvents()
        {
            var data = new ConcurrentObservableDictionary<string, double>();
            var updated = false;
            data.CollectionChanged += (s, e) => { updated = true; };

            data.AddOrUpdate("test", 1.0);

            Assert.IsTrue(updated);
        }

        [TestMethod]
        public void TestAddPartialEvents()
        {
            var data = new ConcurrentObservableDictionary<string, double>();
            var obs = new SimpleObserver();
            data.AddPartialObserver(obs, "test", "test2");

            data.AddOrUpdate("test", 1.0);
            Assert.AreEqual(1.0, obs.LastValue, "Error in test key");

            data.AddOrUpdate("test2", 10.0);
            Assert.AreEqual(10.0, obs.LastValue, "Error in test2 key");
        }

        [TestMethod]
        public void TestAddPartialEventsByAction()
        {
            var data = new ConcurrentObservableDictionary<string, double>();
            var ret = 0.0;
            data.AddPartialObserver(args => ret = args.NewValue, "test", "test2");

            data.AddOrUpdate("test", 1.0);
            Assert.AreEqual(1.0, ret, "Error in test key");

            data.AddOrUpdate("test2", 10.0);
            Assert.AreEqual(10.0, ret, "Error in test2 key");
        }

        [TestMethod]
        public void TestRemovePartialEvents()
        {
            var data = new ConcurrentObservableDictionary<string, double>();
            var obs = new SimpleObserver();
            data.AddPartialObserver(obs, "test", "test2", "test3");

            data.AddOrUpdate("test", 1.0);
            Assert.AreEqual(1.0, obs.LastValue, "Error in test key");

            Assert.IsTrue(data.RemovePartialObserver(obs, "test"), "remove <obs, test>");

            data.AddOrUpdate("test", 10.0);
            Assert.AreEqual(1.0, obs.LastValue, "Error in test key after remove <obs, test>");

            Assert.IsTrue(data.RemovePartialObserver("test3"), "remove test3 key");

            data.AddOrUpdate("test3", 30.0);
            Assert.AreEqual(1.0, obs.LastValue, "Error in test3 key after remove test3 key");

            data.AddOrUpdate("test2", 2.0);
            Assert.AreEqual(2.0, obs.LastValue, "Error in test2 key");

            Assert.IsTrue(data.RemovePartialObserver(obs), "remove obs");

            data.AddOrUpdate("test2", 20.0);
            Assert.AreEqual(2.0, obs.LastValue, "Error in test2 key after remove obs");
        }
    }
}
