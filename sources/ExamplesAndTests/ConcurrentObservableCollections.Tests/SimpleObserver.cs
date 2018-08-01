using ConcurrentObservableCollections.ConcurrentObservableDictionary;

namespace ConcurrentObservableCollections.Tests
{
    public class SimpleObserver : IDictionaryObserver<string, double>
    {
        public double LastValue { get; private set; }

        public void OnEventOccur(DictionaryChangedEventArgs<string, double> args)
        {
            LastValue = args.NewValue;
        }
    }
}
