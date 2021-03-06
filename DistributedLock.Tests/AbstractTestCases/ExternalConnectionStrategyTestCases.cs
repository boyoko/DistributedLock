﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medallion.Threading.Tests.Sql
{
    public abstract class ExternalConnectionStrategyTestCases<TEngineFactory> : TestBase
        where TEngineFactory : ITestingSqlDistributedLockEngineFactory, new()
    {
        [TestMethod]
        public void TestCloseLockOnClosedConnection()
        {
            using (var connection = new SqlConnection(ConnectionStringProvider.ConnectionString))
            using (ConnectionProvider.UseConnection(connection))
            using (var connectionEngine = new TEngineFactory().Create<ConnectionProvider>())
            using (var connectionStringEngine = new TEngineFactory().Create<DefaultConnectionStringProvider>())
            {
                var connectionStringLock = connectionStringEngine.CreateLock(nameof(TestCloseLockOnClosedConnection));

                var @lock = connectionEngine.CreateLock(nameof(TestCloseLockOnClosedConnection));
                TestHelper.AssertThrows<InvalidOperationException>(() => @lock.Acquire());

                connection.Open();

                var handle = @lock.Acquire();
                connectionStringLock.IsHeld().ShouldEqual(true, this.GetType().Name);

                connection.Dispose();

                TestHelper.AssertDoesNotThrow(handle.Dispose);

                // lock can be re-acquired
                connectionStringLock.IsHeld().ShouldEqual(false);
            }
        }

        [TestMethod]
        public void TestIsNotScopedToTransaction()
        {
            using (var connection = new SqlConnection(ConnectionStringProvider.ConnectionString))
            using (ConnectionProvider.UseConnection(connection))
            using (var connectionEngine = new TEngineFactory().Create<ConnectionProvider>())
            using (var connectionStringEngine = new TEngineFactory().Create<DefaultConnectionStringProvider>())
            {
                connection.Open();
                
                using (var handle = connectionEngine.CreateLock(nameof(TestIsNotScopedToTransaction)).Acquire())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        transaction.Rollback();
                    }

                    connectionStringEngine.CreateLock(nameof(TestIsNotScopedToTransaction)).IsHeld().ShouldEqual(true, this.GetType().Name);
                }
            }
        }

        private TestingDistributedLockEngine CreateEngine() => new TEngineFactory().Create<ConnectionProvider>();
    }
}
