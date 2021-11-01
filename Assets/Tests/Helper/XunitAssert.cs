using System;
using NUnit.Framework;

namespace Rx.Unity.Tests.Helper {
    public static class XunitAssert {
        public static T IsType<T>(object @object) {
            Assert.AreEqual(typeof(T), @object.GetType());
            return (T)@object;
        }

        public static void IsType(Type expectedType, object @object) {
            Assert.AreEqual(expectedType, @object.GetType());
        }

        public static void Equal<T>(T expected, T actual) {
            Assert.AreEqual(expected, actual);
        }

        public static void NotEqual<T>(T expected, T actual) {
            Assert.AreNotEqual(expected, actual);
        }
    }
}
