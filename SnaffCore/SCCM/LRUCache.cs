using System;
using System.Collections.Generic;
using System.Threading;

namespace SnaffCore.SCCM
{
    public class LRUCache<TKey, TValue>
    {
        private readonly int _maxSize;
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _cache;
        private readonly LinkedList<CacheItem> _lru;
        private readonly ReaderWriterLockSlim _lock;
        private long _approximateMemoryUsage;
        private readonly long _maxMemoryBytes;

        private class CacheItem
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public long Size { get; set; }
            public DateTime LastAccessed { get; set; }
        }

        public LRUCache(int maxSize = 10000, long maxMemoryMB = 100)
        {
            _maxSize = maxSize;
            _maxMemoryBytes = maxMemoryMB * 1024 * 1024;
            _cache = new Dictionary<TKey, LinkedListNode<CacheItem>>(maxSize);
            _lru = new LinkedList<CacheItem>();
            _lock = new ReaderWriterLockSlim();
            _approximateMemoryUsage = 0;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_cache.TryGetValue(key, out var node))
                {
                    _lock.EnterWriteLock();
                    try
                    {

                        _lru.Remove(node);
                        _lru.AddFirst(node);
                        node.Value.LastAccessed = DateTime.UtcNow;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                    
                    value = node.Value.Value;
                    return true;
                }
                
                value = default(TValue);
                return false;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void Add(TKey key, TValue value, long sizeInBytes = 0)
        {
            _lock.EnterWriteLock();
            try
            {

                if (_cache.TryGetValue(key, out var existingNode))
                {
                    _approximateMemoryUsage -= existingNode.Value.Size;
                    _lru.Remove(existingNode);
                    _cache.Remove(key);
                }


                if (sizeInBytes == 0)
                {
                    sizeInBytes = EstimateSize(value);
                }


                while (_cache.Count >= _maxSize || _approximateMemoryUsage + sizeInBytes > _maxMemoryBytes)
                {
                    if (_lru.Last == null) break;
                    
                    var lruItem = _lru.Last.Value;
                    _cache.Remove(lruItem.Key);
                    _approximateMemoryUsage -= lruItem.Size;
                    _lru.RemoveLast();
                }


                var cacheItem = new CacheItem
                {
                    Key = key,
                    Value = value,
                    Size = sizeInBytes,
                    LastAccessed = DateTime.UtcNow
                };

                var node = _lru.AddFirst(cacheItem);
                _cache[key] = node;
                _approximateMemoryUsage += sizeInBytes;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _cache.Clear();
                _lru.Clear();
                _approximateMemoryUsage = 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _cache.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public long MemoryUsageBytes
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _approximateMemoryUsage;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        private long EstimateSize(TValue value)
        {

            if (value is string str)
            {
                return str.Length * 2;
            }
            

            return 64;
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}