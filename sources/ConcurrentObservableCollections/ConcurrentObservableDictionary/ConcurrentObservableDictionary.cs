using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace ConcurrentObservableCollections.ConcurrentObservableDictionary
{
    public class ConcurrentObservableDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        public event EventHandler<DictionaryChangedEventArgs<TKey, TValue>> CollectionChanged;

        protected virtual void OnCollectionChanged(DictionaryChangedEventArgs<TKey, TValue> changeAction)
        {
            var tasks = new List<Task> { Task.Run(() => CollectionChanged?.Invoke(this, changeAction)) };

            if (_observers.TryGetValue(changeAction.Key, out var observers))
            {
                tasks.AddRange(observers.Select(o => Task.Run(() => o.OnEventOccur(changeAction))));
            }

            Task.WaitAll(tasks.ToArray());
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, TKey key, TValue newValue, TValue oldValue)
        {
            OnCollectionChanged(new DictionaryChangedEventArgs<TKey, TValue>(action, key, newValue, oldValue));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, TKey key, TValue value)
        {
            var newValue = default(TValue);
            var oldValue = default(TValue);
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
            var isUpdated = false;
            var oldValue = default(TValue);

            var value = base.AddOrUpdate(key, addValue, (k, v) =>
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
            var isUpdated = false;
            var oldValue = default(TValue);

            var value = base.AddOrUpdate(key, addValueFactory, (k, v) =>
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
            var isAdded = false;

            var value = base.GetOrAdd(key, k =>
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
            var tryAdd = base.TryAdd(key, value);

            if (tryAdd)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Add, key, value);
            }

            return tryAdd;
        }

        public new bool TryRemove(TKey key, out TValue value)
        {
            var tryRemove = base.TryRemove(key, out value);

            if (tryRemove)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, key, value);
            }

            return tryRemove;
        }

        public new bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            var tryUpdate = base.TryUpdate(key, newValue, comparisonValue);

            if (tryUpdate)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, key, newValue, comparisonValue);
            }

            return tryUpdate;
        }

        public IDictionaryObserver<TKey, TValue> AddPartialObserver(IDictionaryObserver<TKey, TValue> observer, params TKey[] keys)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));
            if (keys is null) throw new ArgumentNullException(nameof(keys));

            foreach (var key in keys)
            {
                _observers.AddOrUpdate(key, new HashSet<IDictionaryObserver<TKey, TValue>> {observer}, (k, o) =>
                {
                    o.Add(observer);
                    return o;
                });
            }

            return observer;
        }

        public IDictionaryObserver<TKey, TValue> AddPartialObserver(
            Action<DictionaryChangedEventArgs<TKey, TValue>> action, params TKey[] keys)
        {
            return AddPartialObserver(new SimpleActionDictionaryObserver<TKey, TValue>(action), keys);
        }

        public Dictionary<TKey, HashSet<IDictionaryObserver<TKey, TValue>>> RemovePartialObserver(
            IDictionaryObserver<TKey, TValue> observer, params TKey[] keys)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));
            if (keys is null) throw new ArgumentNullException(nameof(keys));

            var removed = keys.Where(key => 
                _observers.TryGetValue(key, out var observers) && observers.Contains(observer) && observers.Remove(observer));
            return removed.ToDictionary(k => k, k => new HashSet<IDictionaryObserver<TKey, TValue>> {observer});
        }

        public Dictionary<TKey, HashSet<IDictionaryObserver<TKey, TValue>>> RemovePartialObserver(
            IDictionaryObserver<TKey, TValue> observer)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));

            var removed = _observers.Where(pair => pair.Value.Contains(observer) && pair.Value.Remove(observer)).Select(pair => pair.Key);
            return removed.ToDictionary(k => k, k => new HashSet<IDictionaryObserver<TKey, TValue>> { observer });
        }

        public Dictionary<TKey, HashSet<IDictionaryObserver<TKey, TValue>>> RemovePartialObserver(
            params TKey[] keys)
        {
            if (keys is null) throw new ArgumentNullException(nameof(keys));

            var removed = keys.Select(key =>
            {
                if (_observers.ContainsKey(key) && _observers.TryRemove(key, out var observers))
                {
                    return new KeyValuePair<TKey, HashSet<IDictionaryObserver<TKey, TValue>>>(
                        key, new HashSet<IDictionaryObserver<TKey, TValue>>(observers));
                }
                return new KeyValuePair<TKey, HashSet<IDictionaryObserver<TKey, TValue>>>(key, null);
            });
            return removed.Where(pair => pair.Value != null).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public Dictionary<TKey, HashSet<IDictionaryObserver<TKey, TValue>>> RemoveAllObservers()
        {
            var ret = _observers.ToDictionary(kv => kv.Key, kv => new HashSet<IDictionaryObserver<TKey, TValue>>(kv.Value));
            _observers.Clear();
            return ret;
        }

        #region private data

        private readonly ConcurrentDictionary<TKey, ICollection<IDictionaryObserver<TKey, TValue>>> _observers 
            = new ConcurrentDictionary<TKey, ICollection<IDictionaryObserver<TKey, TValue>>>();

        #endregion
    }
}
