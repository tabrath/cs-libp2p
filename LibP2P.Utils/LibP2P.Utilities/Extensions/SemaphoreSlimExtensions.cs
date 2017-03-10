using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibP2P.Utilities.Extensions
{
    public static class SemaphoreSlimExtensions
    {
        public static void Lock(this SemaphoreSlim semaphore, Action action)
        {
            semaphore.Wait();
            try
            {
                action();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static async Task LockAsync(this SemaphoreSlim semaphore, Action action, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                action();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static T Lock<T>(this SemaphoreSlim semaphore, Func<T> func)
        {
            semaphore.Wait();
            try
            {
                return func();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static async Task<T> LockAsync<T>(this SemaphoreSlim semaphore, Func<T> func, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return func();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
