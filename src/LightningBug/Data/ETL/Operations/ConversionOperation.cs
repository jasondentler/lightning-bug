using System;

namespace LightningBug.Data.ETL.Operations
{
    public class ConversionOperation<TInput, TOutput> : 
        AbstractProjectionOperation<TInput, TOutput>
    {
        private readonly Func<TInput, TOutput> _conversion;

        public ConversionOperation(Func<TInput, TOutput> conversion)
            : this(conversion, string.Format("Convert {0} to {1}", typeof (TInput), typeof (TOutput)))
        {
        }

        public ConversionOperation(Func<TInput, TOutput> conversion, string name) : base(name)
        {
            _conversion = conversion;
        }

        protected override TOutput Execute(TInput input)
        {
            return _conversion(input);
        }
    }
}