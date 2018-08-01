namespace ConcurrentObservableCollections.ConcurrentObservableDictionary
{
    public interface IDictionaryObserver<TKey, TValue>
    {
        void OnEventOccur(DictionaryChangedEventArgs<TKey, TValue> args);
    }
}
