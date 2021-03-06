﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medallion.Threading.Tests
{
    internal static class TestHelper
    {
        public static T ShouldEqual<T>(this T @this, T that, string message = null)
        {
            Assert.AreEqual(actual: @this, expected: that, message: message);
            return @this;
        }

        public static TException AssertThrows<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex)
            {
                return ex;
            }
            catch (Exception ex) 
            {
                Assert.Fail("Expected exception of type " + typeof(TException) + " but found " + ex);
            }

            Assert.Fail("Expected exception of type " + typeof(TException));

            return null; // never gets here
        }

        public static void AssertDoesNotThrow(Action action, string message = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed with " + ex + (message != null ? ": " + message : string.Empty));
            }
        }

        private static volatile Type _currentTestType;

        public static Type CurrentTestType
        {
            get => _currentTestType ?? throw new InvalidOperationException("no test name set");
            set
            {
                var currentTestType = _currentTestType;
                if (value != null && currentTestType != null) { throw new InvalidOperationException("test name not cleared"); }
                if (value == currentTestType) { throw new InvalidOperationException($"bad test name transition from '{currentTestType?.Name ?? "null"}' => '{value?.Name ?? "null"}'"); }
                _currentTestType = value;
            }
        }

        public static bool IsHeld(this IDistributedLock @lock)
        {
            using (var handle = @lock.TryAcquire())
            {
                return handle == null;
            }
        }
    }
}
