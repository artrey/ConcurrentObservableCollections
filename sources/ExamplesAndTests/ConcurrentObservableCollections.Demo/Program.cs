using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using System;

namespace ConcurrentObservableCollections.Demo
{
    class Program
    {
        private static readonly ConcurrentObservableDictionary<string, double> Cache
            = new ConcurrentObservableDictionary<string,double>();

        private static void Main(string[] args)
        {
            Cache.CollectionChanged += (s, e) => { Console.WriteLine($@"{e.Key}: {e.NewValue}"); };
            Cache.AddPartialObserver(e => { Console.WriteLine($@"Auto observer: {e.Key}: {e.NewValue}"); }, "a");
            Cache.AddOrUpdate("a", 22.2);
            Cache.AddOrUpdate("b", 23.2);
            Cache.AddOrUpdate("c", 24.2);
            Cache.AddOrUpdate("a", 22.2);
            Cache.AddOrUpdate("a", 25.2);
        }
    }
}
