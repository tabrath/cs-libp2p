using System;

namespace LibP2P.Utilities.Extensions
{
    public static class TupleExtensions
    {
        public static Tuple<T, T> Swap<T>(this Tuple<T, T> tuple) => new Tuple<T, T>(tuple.Item2, tuple.Item1);
    }
}