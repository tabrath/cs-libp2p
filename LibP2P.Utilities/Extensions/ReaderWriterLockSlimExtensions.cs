using System;
using System.Threading;

namespace LibP2P.Utilities.Extensions
{
    public static class ReaderWriterLockSlimExtensions
    {
        public static void Read(this ReaderWriterLockSlim rwls, Action action, int timeout = Timeout.Infinite)
        {
            if (!rwls.TryEnterReadLock(timeout))
                throw new TimeoutException("Read deadlock");

            try
            {
                action();
            }
            finally
            {
                rwls.ExitReadLock();
            }
        }

        public static T Read<T>(this ReaderWriterLockSlim rwls, Func<T> func, int timeout = Timeout.Infinite)
        {
            if (!rwls.TryEnterReadLock(timeout))
                throw new TimeoutException("Read deadlock");

            try
            {
                return func();
            }
            finally
            {
                rwls.ExitReadLock();
            }
        }

        public static void Write(this ReaderWriterLockSlim rwls, Action action, int timeout = Timeout.Infinite)
        {
            if (!rwls.TryEnterWriteLock(timeout))
                throw new TimeoutException("Write deadlock");

            try
            {
                action();
            }
            finally
            {
                rwls.ExitWriteLock();
            }
        }

        public static T Write<T>(this ReaderWriterLockSlim rwls, Func<T> func, int timeout = Timeout.Infinite)
        {
            if (!rwls.TryEnterWriteLock(timeout))
                throw new TimeoutException("Write deadlock");

            try
            {
                return func();
            }
            finally
            {
                rwls.ExitWriteLock();
            }
        }
    }
}