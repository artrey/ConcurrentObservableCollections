namespace ConcurrentObservableCollections.ConcurrentObservableDictionary
{
    public interface IPartialObservableDictionary<TKey, TValue>
    {
        IDictionaryObserver<TKey, TValue> AddPartialObserver(IDictionaryObserver<TKey, TValue> observer, params TKey[] keys);

        bool RemovePartialObserver(IDictionaryObserver<TKey, TValue> observer, params TKey[] keys);
        bool RemovePartialObserver(IDictionaryObserver<TKey, TValue> observer);
        bool RemovePartialObserver(params TKey[] keys);
    }
}
