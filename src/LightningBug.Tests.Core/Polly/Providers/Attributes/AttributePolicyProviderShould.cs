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

        public class TestPolicy : Policy
        {
            public string Name { get; }
            public int Order { get; }

            public TestPolicy(string name, int order) : base(ExceptionPolicy, Enumerable.Empty<ExceptionPredicate>())
            {
                Name = name;
                Order = order;
            }

            public TestPolicy(string name, int order, bool configureAwait) : base(AsyncExceptionPolicy, Enumerable.Empty<ExceptionPredicate>())
            {
                Name = name;
                Order = order;
            }

            private static Task AsyncExceptionPolicy(Func<Context, CancellationToken, Task> arg1, Context arg2, CancellationToken arg3, bool arg4)
            {
                return Task.CompletedTask;
            }

            private static void ExceptionPolicy(Action<Context, CancellationToken> arg1, Context arg2, CancellationToken arg3)
            {
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

        public class InheritedTestPolicyAttribute : TestPolicyAttribute
        {
        }

        public class TestAttributePolicyProvider : AttributePolicyProvider
        {
            protected override ISyncPolicy GetSyncPolicy(MethodInfo methodInfo, PolicyAttribute attribute)
            {
                var testAttribute = (TestPolicyAttribute) attribute;
                var policy = new TestPolicy(testAttribute.Name, testAttribute.Order);
                return policy;
            }

            protected override IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo, PolicyAttribute attribute)
            {
                var testAttribute = (TestPolicyAttribute)attribute;
                var policy = new TestPolicy(testAttribute.Name, testAttribute.Order, false);
                return policy;
            }

            protected override IEnumerable<PolicyAttribute> CustomSort(IEnumerable<PolicyAttribute> attributes)
            {
                return attributes.Cast<TestPolicyAttribute>().OrderBy(a => a.Name);
            }

        }
        
        [Fact]
        public void ReturnNullWhenMethodHasNoAttribute()
        {
            var mi = typeof(AttributePolicyProviderShould).GetMethod(nameof(NoAttribute));
            var sut = new TestAttributePolicyProvider();
            var policy = sut.GetSyncPolicy(mi);
            policy.ShouldBeNull();
        }

        [Fact]
        public void ReturnsNullWhenMethodHasNoPolicyAttribute()
        {
            var mi = typeof(AttributePolicyProviderShould).GetMethod(nameof(NoAttribute));
            var sut = new TestAttributePolicyProvider();
            var policy = sut.GetSyncPolicy(mi);
            policy.ShouldBeNull();
        }

        [Fact]
        public void ReturnsWrappedPoliciesInOrderSpecified()
        {
            var mi = typeof(AttributePolicyProviderShould).GetMethod(nameof(SortedPolicyAttributes));
            var sut = new TestAttributePolicyProvider();
            var policy = sut.GetSyncPolicy(mi);
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
            var policy = sut.GetSyncPolicy(mi);
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
            var policy = sut.GetSyncPolicy(mi);
            policy.ShouldBeOfType<TestPolicy>();
        }
    }
}
