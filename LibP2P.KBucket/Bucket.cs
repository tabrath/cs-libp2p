using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Peer;

namespace LibP2P.KBucket
{
    public class Bucket
    {
        private readonly List<PeerId> _list;
        private readonly ReaderWriterLockSlim _rwlock;

        public PeerId[] Peers
        {
            get
            {
                _rwlock.EnterReadLock();
                try
                {
                    return _list.ToArray();
                }
                finally
                {
                    _rwlock.ExitReadLock();
                }
            }
        }

        public int Length
        {
            get
            {
                _rwlock.EnterReadLock();
                try
                {
                    return _list.Count;
                }
                finally
                {
                    _rwlock.ExitReadLock();
                }
            }
        }

        public Bucket()
            : this(new List<PeerId>())
        {
        }

        protected Bucket(List<PeerId> list)
        {
            _list = list;
            _rwlock = new ReaderWriterLockSlim();
        }

        public bool Has(PeerId id)
        {
            _rwlock.EnterReadLock();
            try
            {
                return _list.Contains(id);
            }
            finally
            {
                _rwlock.ExitReadLock();
            }
        }

        public void Remove(PeerId id)
        {
            _rwlock.EnterWriteLock();
            try
            {
                _list.Remove(id);
            }
            finally
            {
                _rwlock.ExitWriteLock();
            }
        }

        public void MoveToFront(PeerId id)
        {
            _rwlock.EnterWriteLock();
            try
            {
                _list.Remove(id);
                _list.Insert(0, id);
            }
            finally
            {
                _rwlock.ExitWriteLock();
            }
        }

        public void PushFront(PeerId id)
        {
            _rwlock.EnterWriteLock();
            try
            {
                _list.Insert(0, id);
            }
            finally
            {
                _rwlock.ExitWriteLock();
            }
        }

        public PeerId PopBack()
        {
            _rwlock.EnterWriteLock();
            try
            {
                var last = _list.Last();
                _list.Remove(last);
                return last;
            }
            finally
            {
                _rwlock.ExitWriteLock();
            }
        }

        public Bucket Split(int cpl, DhtId target)
        {
            _rwlock.EnterWriteLock();
            try
            {
                var output = new List<PeerId>();
                var newBuck = new Bucket(output);
                foreach (var e in _list)
                {
                    var peerId = DhtId.ConvertPeerId(e);
                    var peerCpl = DhtId.CommonPrefixLength(peerId, target);

                    if (peerCpl > cpl)
                    {
                        output.Add(e);
                    }
                }

                _list.RemoveAll(output.Contains);

                return newBuck;
            }
            finally
            {
                _rwlock.ExitWriteLock();
            }
        }
    }
}
