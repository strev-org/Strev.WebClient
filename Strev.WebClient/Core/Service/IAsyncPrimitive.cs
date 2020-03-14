using System;

namespace Strev.WebClient.Core.Service
{
    internal interface IAsyncPrimitive<T>
    {
        T ExecuteSync(TimeSpan timeout);
    }
}