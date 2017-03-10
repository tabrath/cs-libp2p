using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LibP2P.Crypto;
using LibP2P.Utilities.Extensions;

namespace LibP2P.Peer.Store
{
    public class KeyBook : IKeyBook
    {
        private readonly Dictionary<PeerId, PublicKey> _pks;
        private readonly Dictionary<PeerId, PrivateKey> _sks;
        private readonly SemaphoreSlim _sync;

        public PeerId[] Peers => _sync.Lock(() => _pks.Keys.Concat(_sks.Keys).Distinct().ToArray());

        internal KeyBook()
        {
            _pks = new Dictionary<PeerId, PublicKey>();
            _sks = new Dictionary<PeerId, PrivateKey>();
            _sync = new SemaphoreSlim(1, 1);
        }

        public PublicKey PublicKey(PeerId p) => _sync.Lock(() => _pks[p]);

        public void AddPublicKey(PeerId p, PublicKey pk)
        {
            if (!p.MatchesPublicKey(pk))
                throw new Exception("Id does not match PublicKey");

            _sync.Lock(() => _pks[p] = pk);
        }

        public PrivateKey PrivateKey(PeerId p) => _sync.Lock(() => _sks[p]);

        public void AddPrivateKey(PeerId p, PrivateKey sk)
        {
            if (sk == null)
                throw new ArgumentNullException(nameof(sk));

            if (!p.MatchesPrivateKey(sk))
                throw new Exception("Id does not match PrivateKey");

            _sync.Lock(() => _sks[p] = sk);
        }
    }
}