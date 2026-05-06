using System;
using System.Reflection;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    public class RimChatApiShimTests
    {
        public RimChatApiShimTests()
        {
            RimChatDetector.IsRimChatApiAvailable = false;
            RimChatDetector.IsRimChatActive = false;
        }

        [Fact]
        public void ApiType_WhenRimChatNotLoaded_ReturnsNull()
        {
            var result = RimChatApiShim.ApiType;
            Assert.Null(result);
        }

        [Fact]
        public void DiplomacyManagerType_WhenRimChatNotLoaded_ReturnsNull()
        {
            var result = RimChatApiShim.DiplomacyManagerType;
            Assert.Null(result);
        }

        [Fact]
        public void RpgArchiveManagerType_WhenRimChatNotLoaded_ReturnsNull()
        {
            var result = RimChatApiShim.RpgArchiveManagerType;
            Assert.Null(result);
        }

        [Fact]
        public void IsAvailable_DelegatesToDetector()
        {
            RimChatDetector.IsRimChatApiAvailable = false;
            Assert.False(RimChatApiShim.IsAvailable);

            RimChatDetector.IsRimChatApiAvailable = true;
            Assert.True(RimChatApiShim.IsAvailable);
        }

        [Fact]
        public void TypeResolution_IsCached()
        {
            var first = RimChatApiShim.ApiType;
            var second = RimChatApiShim.ApiType;
            Assert.Same(first, second);
        }

        [Fact]
        public void GetStaticPropertyValue_ValidType_ReturnsValue()
        {
            var type = typeof(TestClass);
            var result = RimChatApiShim.GetStaticPropertyValue(type, "StaticProp");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void GetStaticPropertyValue_MissingProperty_ReturnsNull()
        {
            var type = typeof(TestClass);
            var result = RimChatApiShim.GetStaticPropertyValue(type, "NonExistent");
            Assert.Null(result);
        }

        [Fact]
        public void GetStaticPropertyValue_NullType_ReturnsNull()
        {
            var result = RimChatApiShim.GetStaticPropertyValue(null!, "Anything");
            Assert.Null(result);
        }

        [Fact]
        public void GetInstanceFieldValue_ValidField_ReturnsValue()
        {
            var instance = new TestClass { PublicField = "world" };
            var result = RimChatApiShim.GetInstanceFieldValue(instance, "PublicField");
            Assert.Equal("world", result);
        }

        [Fact]
        public void GetInstanceFieldValue_PrivateField_WithCorrectFlags_ReturnsValue()
        {
            var instance = new TestClass();
            typeof(TestClass).GetField("_privateField",
                BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(instance, "secret");

            var result = RimChatApiShim.GetInstanceFieldValue(instance, "_privateField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.Equal("secret", result);
        }

        [Fact]
        public void GetInstanceFieldValue_MissingField_ReturnsNull()
        {
            var instance = new TestClass();
            var result = RimChatApiShim.GetInstanceFieldValue(instance, "NonExistent");
            Assert.Null(result);
        }

        [Fact]
        public void GetInstanceFieldValue_PrivateField_WithoutNonPublicFlags_ReturnsNull()
        {
            var instance = new TestClass();
            typeof(TestClass).GetField("_privateField",
                BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(instance, "secret");

            var result = RimChatApiShim.GetInstanceFieldValue(instance, "_privateField");
            Assert.Null(result);
        }

        private class TestClass
        {
            public static string StaticProp => "hello";
            public string PublicField = "";
#pragma warning disable CS0414
            private string _privateField = "";
#pragma warning restore CS0414
        }
    }
}
