using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Wrap;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Providers.Attributes
{
    public class AttributePolicyProviderShould
    {

        public void NoAttribute()
        {
        }

        [Description("This method has no policy attribute")]
        public void NonPolicyAttribute()
        {
        }

        [TestPolicy]
        public void PolicyAttribute()
        {
        }

        [TestPolicy(Name = "First", Order = 1)]
        [TestPolicy(Name = "Second", Order = 2)]
        public void SortedPolicyAttributes()
        {
        }

        [TestPolicy(Name = "First")]
        [TestPolicy(Name = "Second")]
        public void NamedPolicyAttributes()
        {
        }

        [InheritedTestPolicy]
        public void UsesInheritedAttribute()
        {
        }

        private class AsyncTestPolicy : AsyncPolicy
        {
            
            public string Name { get; }
            public int Order { get; }

            public AsyncTestPolicy(string name, int order)
            {
                Name = name;
                Order = order;
            }

            public AsyncTestPolicy(string name, int order, bool configureAwait)
            {
                Name = name;
                Order = order;
            }

            protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
                bool continueOnCapturedContext)
            {
                throw new NotImplementedException();
            }
        }

        private class TestPolicy : Policy
        {
            public string Name { get; }
            public int Order { get; }

            public TestPolicy(string name, int order)
            {
                Name = name;
                Order = order;
            }

            public TestPolicy(string name, int order, bool configureAwait)
            {
                Name = name;
                Order = order;
            }

            protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public class TestPolicyAttribute : PolicyAttribute
        {
            public string Name { get; set; }
            public int Order { get; set; }

            protected internal override int GetOrder()
            {
                return Order;
            }
        }

        public class SpecificAttributePolicyProvider : IAttributePolicyProvider
        {
            public ISyncPolicy GetSyncPolicy(CallContextBase context, PolicyAttribute attribute)
            {
                return null;
            }

            public IAsyncPolicy GetAsyncPolicy(CallContextBase context, PolicyAttribute attribute)
            {
                return null;
            }
        }

        public class InheritedTestPolicyAttribute : TestPolicyAttribute
        {
        }

        public class TestAttributePolicyProvider : AttributePolicyProvider
        {
            public TestAttributePolicyProvider() : base(new SpecificAttributePolicyProvider())
            {
            }

            protected override ISyncPolicy GetSyncPolicy(CallContextBase context, PolicyAttribute attribute)
            {
                var testAttribute = (TestPolicyAttribute) attribute;
                var policy = new TestPolicy(testAttribute.Name, testAttribute.Order);
                return policy;
            }

            protected override IAsyncPolicy GetAsyncPolicy(CallContextBase context, PolicyAttribute attribute)
            {
                var testAttribute = (TestPolicyAttribute)attribute;
                var policy = new AsyncTestPolicy(testAttribute.Name, testAttribute.Order, false);
                return policy;
            }

            protected override IEnumerable<PolicyAttribute> CustomSort(IEnumerable<PolicyAttribute> attributes)
            {
                return attributes.Cast<TestPolicyAttribute>().OrderBy(a => a.Name);
            }

        }

        private CallContextBase GetTestContext(MethodInfo mi)
        {
            return new CallContext(this.GetType(), this, mi, new object[0]);
        }

        [Fact]
        public void ReturnNullWhenMethodHasNoAttribute()
        {
            var mi = typeof(AttributePolicyProviderShould).GetMethod(nameof(NoAttribute));
            var sut = new TestAttributePolicyProvider();
            var policy = sut.GetSyncPolicy(GetTestContext(mi));
            policy.ShouldBeNull();
        }

        [Fact]
        public void ReturnsNullWhenMethodHasNoPolicyAttribute()
        {
            var mi = typeof(AttributePolicyProviderShould).GetMethod(nameof(NoAttribute));
            var sut = new TestAttributePolicyProvider();
            var policy = sut.GetSyncPolicy(GetTestContext(mi));
            policy.ShouldBeNull();
        }

        [Fact]
        public void ReturnsWrappedPoliciesInOrderSpecified()
        {
            var mi = typeof(AttributePolicyProviderShould).GetMethod(nameof(SortedPolicyAttributes));
            var sut = new TestAttributePolicyProvider();
            var policy = sut.GetSyncPolicy(GetTestContext(mi));
            policy.ShouldBeOfType<PolicyWrap>();

            var policyWrap = policy as PolicyWrap;
            var policies = policyWrap.GetPolicies().OfType<TestPolicy>().ToArray();
            policies.Select(p => p.Order).ShouldBe(new[] {1, 2});
            policies.Select(p => p.Name).ShouldBe(new []{"First", "Second"});
        }

        [Fact]
        public void ReturnsWrappedPoliciesSortedByName()
        {
            var mi = typeof(AttributePolicyProviderShould).GetMethod(nameof(NamedPolicyAttributes));
            var sut = new TestAttributePolicyProvider();
            var policy = sut.GetSyncPolicy(GetTestContext(mi));
            policy.ShouldBeOfType<PolicyWrap>();

            var policyWrap = policy as PolicyWrap;
            var policies = policyWrap.GetPolicies().OfType<TestPolicy>().ToArray();
            policies.Select(p => p.Order).ShouldBe(new[] {0, 0});
            policies.Select(p => p.Name).ShouldBe(new[] { "First", "Second" });
        }

        [Fact]
        public void ReturnsPolicyWhenAttributeIsInherited()
        {
            var mi = typeof(AttributePolicyProviderShould).GetMethod(nameof(UsesInheritedAttribute));
            var sut = new TestAttributePolicyProvider();
            var policy = sut.GetSyncPolicy(GetTestContext(mi));
            policy.ShouldBeOfType<TestPolicy>();
        }
    }
}
