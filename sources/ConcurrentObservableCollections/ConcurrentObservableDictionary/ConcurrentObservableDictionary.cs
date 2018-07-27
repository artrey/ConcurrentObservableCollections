using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ConcurrentObservableCollections.ConcurrentObservableDictionary
{
    public class ConcurrentObservableDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        public event EventHandler<DictionaryChangedEventArgs<TKey, TValue>> CollectionChanged;

        protected virtual void OnCollectionChanged(DictionaryChangedEventArgs<TKey, TValue> changeAction)
        {
            CollectionChanged?.Invoke(this, changeAction);
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, TKey key, TValue newValue, TValue oldValue)
        {
            OnCollectionChanged(new DictionaryChangedEventArgs<TKey, TValue>(action, key, newValue, oldValue));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, TKey key, TValue value)
        {
            TValue newValue = default(TValue);
            TValue oldValue = default(TValue);
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    newValue = value;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    oldValue = value;
                    break;
                default:
                    return;
            }
            OnCollectionChanged(action, key, newValue, oldValue);
        }

        #region Ctors
        public ConcurrentObservableDictionary()
        {

        }

        public ConcurrentObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : base(collection)
        {

        }

        public ConcurrentObservableDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {

        }

        public ConcurrentObservableDictionary(int concurrencyLevel, int capacity)
            : base(concurrencyLevel, capacity)
        {

        }

        public ConcurrentObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : base(collection, comparer)
        {

        }

        public ConcurrentObservableDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
            : base(concurrencyLevel, capacity, comparer)
        {

        }

        public ConcurrentObservableDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : base(concurrencyLevel, collection, comparer)
        {

        }
        #endregion

        public new void Clear()
        {
            base.Clear();
            OnCollectionChanged(new DictionaryChangedEventArgs<TKey, TValue>(NotifyCollectionChangedAction.Reset));
        }

        public new TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            bool isUpdated = false;
            TValue oldValue = default(TValue);

            TValue value = base.AddOrUpdate(key, addValue, (k, v) =>
            {
                isUpdated = true;
                oldValue = v;
                return updateValueFactory(k, v);
            });

            if (isUpdated && !value.Equals(oldValue))
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, key, value, oldValue);
            }
            else if (!isUpdated)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Add, key, value);
            }

            return value;
        }

        public new TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            bool isUpdated = false;
            TValue oldValue = default(TValue);

            TValue value = base.AddOrUpdate(key, addValueFactory, (k, v) =>
            {
                isUpdated = true;
                oldValue = v;
                return updateValueFactory(k, v);
            });

            if (isUpdated && !value.Equals(oldValue))
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, key, value, oldValue);
            }
            else if (!isUpdated)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Add, key, value);
            }

            return value;
        }

        public TValue AddOrUpdate(TKey key, TValue value)
        {
            return AddOrUpdate(key, value, (k, v) => value);
        }

        public new TValue GetOrAdd(TKey key, TValue value)
        {
            return GetOrAdd(key, k => value);
        }

        public new TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            bool isAdded = false;

            TValue value = base.GetOrAdd(key, k =>
            {
                isAdded = true;
                return valueFactory(k);
            });

            if (isAdded)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Add, key, value);
            }

            return value;
        }

        public new bool TryAdd(TKey key, TValue value)
        {
            bool tryAdd = base.TryAdd(key, value);

            if (tryAdd)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Add, key, value);
            }

            return tryAdd;
        }

        public new bool TryRemove(TKey key, out TValue value)
        {
            bool tryRemove = base.TryRemove(key, out value);

            if (tryRemove)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, key, value);
            }

            return tryRemove;
        }

        public new bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            bool tryUpdate = base.TryUpdate(key, newValue, comparisonValue);

            if (tryUpdate)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, key, newValue, comparisonValue);
            }

            return tryUpdate;
        }
    }
}
