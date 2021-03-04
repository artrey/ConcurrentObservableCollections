using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using System;

namespace ConcurrentObservableCollections.Demo
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            ConcurrentObservableDictionary<string, double> data = new();
            
            data.CollectionChanged += (_, e) => { Console.WriteLine($@"{e.Key}: {e.NewValue}"); };
            data.AddPartialObserver(e => { Console.WriteLine($@"Auto observer: {e.Key}: {e.NewValue}"); }, "a");
            
            data.AddOrUpdate("a", 22.2);
            data.AddOrUpdate("b", 23.2);
            data.AddOrUpdate("c", 24.2);
            data.AddOrUpdate("a", 22.2);
            data.AddOrUpdate("a", 25.2);
        }
    }
}