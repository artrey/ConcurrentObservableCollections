using System;
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

        public DictionaryChangedEventArgs(NotifyCollectionChangedAction action, TKey key, TValue value)
            : this(action)
        {
            Key = key;
            if (action == NotifyCollectionChangedAction.Add)
            {
                NewValue = value;
            }
            else if (action == NotifyCollectionChangedAction.Remove)
            {
                OldValue = value;
            }
            else
            {
                throw new ArgumentException(
                    $@"Action may be only '{NotifyCollectionChangedAction.Add}' or '{NotifyCollectionChangedAction.Remove}'. Got: '{action}'");
            }
        }

        public NotifyCollectionChangedAction Action { get; }
        public TKey Key { get; }
        public TValue NewValue { get; }
        public TValue OldValue { get; }
    }
}
