using System.Collections.Generic;
using LibP2P.Crypto;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;

namespace LibP2P.Record
{
    public class Validator
    {
        private readonly Dictionary<string, ValidChecker> _checkers;

        public Validator()
            : this(new Dictionary<string, ValidChecker>())
        {
        }

        public Validator(Dictionary<string, ValidChecker> checkers)
        {
            _checkers = checkers;
        }

        public bool VerifyRecord(DHTRecord record)
        {
            var parts = record.Key.Split('/');
            if (parts.Length < 3)
                return true;

            if (!_checkers.ContainsKey(parts[1]))
                return false;

            return _checkers[parts[1]].Func(record.Key, record.Value);
        }

        public bool IsSigned(string key)
        {
            var parts = key.Split('/');
            if (parts.Length < 3)
                return false;

            if (!_checkers.ContainsKey(parts[1]))
                return false;

            return _checkers[parts[1]].Sign;
        }

        public static ValidChecker PublicKeyValidator = new ValidChecker
        {
            Func = ValidatePublicKeyRecord,
            Sign = false
        };

        public static bool ValidatePublicKeyRecord(string key, byte[] value)
        {
            if (key.Length < 5)
                return false;

            var prefix = key.Substring(0, 4);
            if (prefix != "/pk/")
                return false;

            Multihash mh;
            if (!Multihash.TryParse(key.Substring(4), out mh))
                return false;

            var pkh = Multihash.Sum<SHA2_256>(value);

            return mh.Equals(pkh);
        }

        public static bool CheckRecordSignature(DHTRecord record, PublicKey pk) => pk.Verify(record.GetBlob(), record.Signature);
    }
}