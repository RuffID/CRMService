using CRMService.Infrastructure.DataBase.Repository;
using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Database
{
    public class MainDbUnitOfWorkScopeTests
    {
        [Fact]
        public async Task SaveChangesAsync_CancellationToken_DelegatesToSharedSession()
        {
            FakeMainDbContextSession session = new();
            MainDbUnitOfWorkScope scope = new(session);
            using CancellationTokenSource cancellation = new();

            await scope.SaveChangesAsync(cancellation.Token);

            Assert.Equal(1, session.SaveCount);
            Assert.Equal(cancellation.Token, session.LastCancellationToken);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_Action_DelegatesToSharedSession()
        {
            FakeMainDbContextSession session = new();
            MainDbUnitOfWorkScope scope = new(session);
            bool actionExecuted = false;

            await scope.ExecuteInTransactionAsync(
                _ =>
                {
                    actionExecuted = true;
                    return Task.CompletedTask;
                },
                TestContext.Current.CancellationToken);

            Assert.True(actionExecuted);
            Assert.Equal(1, session.TransactionCount);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_ActionFails_PropagatesOriginalException()
        {
            FakeMainDbContextSession session = new();
            MainDbUnitOfWorkScope scope = new(session);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                scope.ExecuteInTransactionAsync(
                    _ => throw new InvalidOperationException("failure"),
                    TestContext.Current.CancellationToken));

            Assert.Equal("failure", exception.Message);
            Assert.Equal(1, session.TransactionCount);
        }

        private class FakeMainDbContextSession : IMainDbContextSession
        {
            public int SaveCount { get; private set; }
            public int TransactionCount { get; private set; }
            public CancellationToken LastCancellationToken { get; private set; }

            public Task SaveChangesAsync(CancellationToken ct = default)
            {
                SaveCount++;
                LastCancellationToken = ct;
                return Task.CompletedTask;
            }

            public async Task ExecuteInTransactionAsync(
                Func<CancellationToken, Task> action,
                CancellationToken ct = default)
            {
                TransactionCount++;
                LastCancellationToken = ct;
                await action(ct);
            }
        }
    }
}
