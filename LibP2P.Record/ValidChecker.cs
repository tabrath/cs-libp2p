using System;

namespace LibP2P.Record
{
    public class ValidChecker
    {
        public Func<string, byte[], bool> Func { get; set; }
        public bool Sign { get; set; }
    }
}