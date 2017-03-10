using System;
using System.Collections.Generic;

namespace LibP2P.Record
{
    public class Selector
    {
        private readonly Dictionary<string, Func<string, byte[][], int>> _selectors;

        public Selector()
            : this(new Dictionary<string, Func<string, byte[][], int>>())
        {
        }

        public Selector(Dictionary<string, Func<string, byte[][], int>> selectors)
        {
            _selectors = selectors;
        }

        public int BestRecord(string key, params byte[][] records)
        {
            if (records.Length == 0)
                return 0;

            var parts = key.Split('/');
            if (parts.Length < 3)
                return 0;

            Func<string, byte[][], int> sel;
            if (!_selectors.TryGetValue(parts[1], out sel))
                return 0;

            return sel(key, records);
        }

        public static int PublicKeySelector(string key, params byte[][] records)
        {
            return 0;
        }
    }
}