using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Polly;
using Polly.NoOp;

namespace LightningBug.Polly.Wrapper
{
    public class NullPolicyProvider : IPolicyProvider
    {
        public ISyncPolicy GetSyncPolicy()
        {
            return null;
        }

        public IAsyncPolicy GetAsyncPolicy()
        {
            return null;
        }
    }
}