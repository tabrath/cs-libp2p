using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LibP2P.Utilities.Extensions;

namespace LibP2P.Utilities
{
    public class ConcurrentList<T> : IList<T>, IDisposable
    {
        private readonly List<T> _list;
        private readonly ReaderWriterLockSlim _lock;

        public int Count => _lock.Read(() => _list.Count);
        public bool IsReadOnly => false;
        public T this[int index]
        {
            get { return _lock.Read(() => _list[index]); }
            set { _lock.Write(() => _list[index] = value); }
        }

        public ConcurrentList()
        {
            _list = new List<T>();
            _lock = new ReaderWriterLockSlim();
        }

        public bool Contains(T item) => _lock.Read(() => _list.Contains(item));
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator() => _lock.Read(() => _list.ToList()).GetEnumerator();
        public void Add(T item) => _lock.Write(() => _list.Add(item));
        public void Clear() => _lock.Write(() => _list.Clear());
        public void CopyTo(T[] array, int arrayIndex) => _lock.Read(() => _list.CopyTo(array, arrayIndex));
        public bool Remove(T item) => _lock.Write(() => _list.Remove(item));
        public int IndexOf(T item) => _lock.Read(() => _list.IndexOf(item));
        public void Insert(int index, T item) => _lock.Write(() => _list.Insert(index, item));
        public void RemoveAt(int index) => _lock.Write(() => _list.RemoveAt(index));
        public void Dispose() => _lock?.Dispose();
    }
}
