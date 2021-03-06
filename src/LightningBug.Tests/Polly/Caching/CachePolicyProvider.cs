﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using LightningBug.Polly.Providers;
using Moq;
using Polly.Caching;
using Shouldly;
using Xunit;
using CallContext = LightningBug.Polly.Providers.CallContext;

namespace LightningBug.Polly.Caching
{
    public class AtrributeCachingPolicyProviderShould
    {

        public interface ITestInterface
        {

            [SingleCacheEntry("single-cache-entry")]
            int SingleCacheEntry();

            [CacheEntry("cache-entry", "parameter")]
            int CacheEntry(int parameter);
        }

        public class TestInterfaceImplementation : ITestInterface
        {
            public int SingleCacheEntry()
            {
                throw new NotImplementedException();
            }

            public int CacheEntry(int parameter)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void CacheWhenUsingASingleCacheEntryAttribute()
        {
            var syncCacheProvider = new Mock<ISyncCacheProvider>();
            var asyncCacheProvider = new Mock<IAsyncCacheProvider>();
            var sut = new AttributeCachingPolicyProvider(syncCacheProvider.Object, asyncCacheProvider.Object);
            var method = typeof(ITestInterface).GetMethod("SingleCacheEntry");
            var context = new CallContext(typeof(ITestInterface), new TestInterfaceImplementation(), method, new object[0]);
            var attribute = method.GetCustomAttributes().OfType<CacheAttribute>().Single();
            var policy = sut.GetAsyncPolicy(context, attribute);
            policy.ShouldNotBeNull();
        }

        [Fact]
        public void CacheWhenUsingACacheEntryAttribute()
        {
            var syncCacheProvider = new Mock<ISyncCacheProvider>();
            var asyncCacheProvider = new Mock<IAsyncCacheProvider>();
            var sut = new AttributeCachingPolicyProvider(syncCacheProvider.Object, asyncCacheProvider.Object);
            var method = typeof(ITestInterface).GetMethod("CacheEntry");
            var context = new CallContext(typeof(ITestInterface), new TestInterfaceImplementation(), method, new object[] {0});
            var attribute = method.GetCustomAttributes().OfType<CacheAttribute>().Single();
            var policy = sut.GetAsyncPolicy(context, attribute);
            policy.ShouldNotBeNull();
        }

    }
}
