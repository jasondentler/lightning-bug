using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using LightningBug.Polly.CircuitBreaker;
using LightningBug.Polly.Retry;

namespace LightningBug.Polly.SimpleWebAppExample.Services
{

    public interface IValuesProvider
    {
        [Retry(3, Order = 0)]
        [CircuitBreaker(DurationOfBreakInMinutes = 0.25, ExceptionsAllowedBeforeBreaking = 2, Order = 1)]
        IEnumerable<string> GetValues();
    }

    public class ValuesProvider : IValuesProvider
    {

        private static int _callCount = 0;

        public IEnumerable<string> GetValues()
        {
            _callCount++;
            if (_callCount % 4 == 0)
                return new string[] {$"value1 (call count: {_callCount})", "value2", "value3"};

            throw new ApplicationException($"Call count is {_callCount}, not divisible by 4.");
        }
    }
}
