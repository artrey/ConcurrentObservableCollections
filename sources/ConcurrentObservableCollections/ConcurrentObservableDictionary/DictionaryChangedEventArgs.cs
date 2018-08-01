using System.Collections.Specialized;

namespace ConcurrentObservableCollections.ConcurrentObservableDictionary
{
    public class DictionaryChangedEventArgs<TKey, TValue>
    {
        public DictionaryChangedEventArgs(NotifyCollectionChangedAction action)
        {
            Action = action;
        }

        public DictionaryChangedEventArgs(NotifyCollectionChangedAction action, TKey key, TValue newValue, TValue oldValue)
            : this(action)
        {
            Key = key;
            NewValue = newValue;
            OldValue = oldValue;
        }

        public NotifyCollectionChangedAction Action { get; }
        public TKey Key { get; }
        public TValue NewValue { get; }
        public TValue OldValue { get; }
    }
}
