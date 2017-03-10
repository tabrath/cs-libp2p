using Multiformats.Hash;

namespace LibP2P.Utilities.Extensions
{
    public static class MultihashExtensions
    {
        public static int Compare(this Multihash a, Multihash b) => ((byte[])a).Compare((byte[])b);
    }
}