using System;
using System.Collections.Generic;
using System.IO;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper
{
    public class PollyWrapperShould
    {



        public interface IService
        {
            string Id { get; }
            string Name { get; set; }
            int this[int i] { get; }
            string this[string s] { get; set; }
            void ThrowApplicationException();
            void SayHelloWorld();
            void Say(string message);
            string EchoHelloWorld();
            string Echo(string message);
        }

        public class Service : IService
        {
            public const string HelloWorld = "Hello World!";

            private readonly TextWriter _output;

            public Service() : this(Console.Out)
            {
            }

            public Service(System.IO.TextWriter output)
            {
                Id = Guid.Empty.ToString();
                Name = "John Smith";
                _output = output;
            }

            public void ThrowApplicationException()
            {
                throw new ApplicationException();
            }

            public void SayHelloWorld()
            {
                Say(HelloWorld);
            }

            public void Say(string message)
            {
                _output.Write(message);
            }

            public string EchoHelloWorld()
            {
                return Echo(HelloWorld);
            }

            public string Echo(string message)
            {
                return message;
            }

            public string Id { get; }
            public string Name { get; set; }

            public int this[int i] => i;

            private readonly IDictionary<string, string> _indexedStrings = new Dictionary<string, string>();
            public string this[string s]
            {
                get => _indexedStrings[s];
                set => _indexedStrings[s] = value;
            }

        }

        [Fact]
        public void ThrowWhenWrappingAClass()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            Assert.Throws<TypeInitializationException>(() => PollyWrapper<Service>.Decorate(impl, provider));
        }

        [Fact]
        public void WrapASimpleInterface()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            var proxyAsInterface = PollyWrapper<IService>.Decorate(impl, provider);
        }

        [Fact]
        public void InitializeTheUnderlyingServiceProperty()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            var proxyAsInterface = PollyWrapper<IService>.Decorate(impl, provider);
            var proxyAsProxy = (Proxy<IService, TestPolicyProvider>) proxyAsInterface;
            proxyAsProxy.Service.ShouldBeSameAs(impl);
        }

        [Fact]
        public void InitializeThePolicyProviderProperty()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            var proxyAsInterface = PollyWrapper<IService>.Decorate(impl, provider);
            var proxyAsProxy = (Proxy<IService, TestPolicyProvider>)proxyAsInterface;
            proxyAsProxy.PolicyProvider.ShouldBeSameAs(provider);
        }


    }
}
