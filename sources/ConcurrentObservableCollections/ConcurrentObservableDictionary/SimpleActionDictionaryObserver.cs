using System;

namespace ConcurrentObservableCollections.ConcurrentObservableDictionary
{
    internal class SimpleActionDictionaryObserver<TKey, TValue> : IDictionaryObserver<TKey, TValue>
    {
        private readonly Action<DictionaryChangedEventArgs<TKey, TValue>> _action;

        public SimpleActionDictionaryObserver(Action<DictionaryChangedEventArgs<TKey, TValue>> action)
        {
            _action = action;
        }

        public void OnEventOccur(DictionaryChangedEventArgs<TKey, TValue> args)
        {
            _action.Invoke(args);
        }
    }
}
