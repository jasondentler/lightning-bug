using System;

namespace LightningBug.Polly.Caching
{
    public class MissingArgumentException : ArgumentException 
    {
        private readonly string _argumentName;

        public MissingArgumentException(string argumentName)
        {
            _argumentName = argumentName;
        }

        public override string Message => $"Argument {_argumentName} missing from CallContext arguments";
    }
}