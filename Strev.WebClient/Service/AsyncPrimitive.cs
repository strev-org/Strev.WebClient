using Strev.WebClient.Core.Service;
using Strev.WebClient.Exceptions;
using System;
using System.Threading;

namespace Strev.WebClient.Service
{
    internal class AsyncPrimitive<T> : IAsyncPrimitive<T>
    {
        private readonly Func<AsyncCallback, object, IAsyncResult> _begin;
        private readonly Func<IAsyncResult, T> _end;

        private Exception _exception;
        private EventWaitHandle _syncWH;
        private T _result;

        public AsyncPrimitive(Func<AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end)
        {
            _begin = begin;
            _end = end;
        }

        public T ExecuteSync(TimeSpan timeout)
        {
            using (_syncWH = new AutoResetEvent(false))
            {
                _begin(Callback, null);

                if (_syncWH.WaitOne(timeout))
                {
                    // signaled
                    if (_exception == null)
                    {
                        return _result;
                    }
                    throw _exception;
                }
                // timeout
                throw new AsyncTimeoutException("Master timeout");
            }
        }

        private void Callback(IAsyncResult r)
        {
            try
            {
                _result = _end(r);
            }
            catch (Exception e)
            {
                _exception = e;
            }
            finally
            {
                try
                {
                    _syncWH.Set();
                }
                catch (ObjectDisposedException)
                {
                    // When the timeout already expired, the waitHandle may have been disposed already
                    // we ignore it.
                }
            }
        }
    }
}