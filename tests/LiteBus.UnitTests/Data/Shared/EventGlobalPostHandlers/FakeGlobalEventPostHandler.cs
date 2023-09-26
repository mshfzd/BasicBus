using System.Threading;
using System.Threading.Tasks;
using LiteBus.Events.Abstractions;
using LiteBus.UnitTests.Data.Shared.Events;

namespace LiteBus.UnitTests.Data.Shared.EventGlobalPostHandlers;

public sealed class FakeGlobalEventPostHandler : IEventPostHandler
{
    public Task PostHandleAsync(IEvent message, CancellationToken cancellationToken = default)
    {
        (message as FakeParentEvent)!.ExecutedTypes.Add(typeof(FakeGlobalEventPostHandler));
        return Task.CompletedTask;
    }
}