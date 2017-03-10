using System;
using System.Threading;

namespace LibP2P.Routing.Notifications
{
    public class QueryContext : IDisposable
    {
        private readonly Action<QueryContext, QueryEvent> _event;
        private readonly ManualResetEvent _done;

        public WaitHandle Done => _done;

        protected QueryContext(Action<QueryContext, QueryEvent> @event)
        {
            _event = @event;
            _done = new ManualResetEvent(false);
        }

        ~QueryContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _done?.Set();

            if (disposing)
            {
                _done?.Dispose();
            }
        }

        public void PublishQueryEvent(QueryEvent ev)
        {
            if (_done.WaitOne(1))
                return;

            _event?.Invoke(this, ev);
        }

        public static QueryContext RegisterForQueryEvents(Action<QueryContext, QueryEvent> @event) => new QueryContext(@event);
    }
}