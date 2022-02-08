﻿using System;
using System.Linq;
using LiteBus.Messaging.Abstractions;
using LiteBus.Messaging.Abstractions.Metadata;
using LiteBus.Messaging.Workflows.Execution.Exceptions;
using LiteBus.Messaging.Workflows.Extensions;

namespace LiteBus.Messaging.Workflows.Execution;

public class SingleSyncHandlerExecutionWorkflow<TMessage> : IExecutionWorkflow<TMessage, NoResult>
{
    public NoResult Execute(TMessage message, IMessageContext messageContext)
    {
        var handlers = messageContext.Handlers
                                     .Where(h => h.Descriptor.ExecutionMode == ExecutionMode.Synchronous)
                                     .ToList();

        if (handlers.Count > 1)
        {
            throw new MultipleHandlerFoundException(typeof(TMessage));
        }

        var handleContext = new HandleContext(message, default);

        try
        {
            messageContext.RunSyncPreHandlers(handleContext);

            var handler = handlers.Single().Instance;

            handler.Handle(handleContext);

            messageContext.RunSyncPostHandlers(handleContext);
        }
        catch (Exception e)
        {
            if (messageContext.ErrorHandlers.Count + messageContext.IndirectErrorHandlers.Count == 0)
            {
                throw;
            }

            handleContext.Exception = e;

            messageContext.RunSyncErrorHandlers(handleContext);
        }

        return new NoResult();
    }
}