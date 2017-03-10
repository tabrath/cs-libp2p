using System;
using System.Threading;

namespace LibP2P.Utilities
{
    public abstract class SyncMutex : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;
        private bool _disposed;

        protected SyncMutex()
        {
            _lock = new ReaderWriterLockSlim();
        }

        public T Read<T>(Func<T> func)
        {
            _lock.EnterReadLock();
            try
            {
                return func();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Read(Action action)
        {
            _lock.EnterReadLock();
            try
            {
                action();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public T Write<T>(Func<T> func)
        {
            _lock.EnterWriteLock();
            try
            {
                return func();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Write(Action action)
        {
            _lock.EnterWriteLock();
            try
            {
                action();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _lock.Dispose();
                _disposed = true;
            }
        }
    }
}