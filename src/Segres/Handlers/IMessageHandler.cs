﻿using Segres.Contracts;

namespace Segres.Handlers;

/// <summary>
/// Defines a subscriber for a notification.
/// </summary>
/// <seealso cref="IMessage"/>
public interface IMessageHandler<in TMessage> 
    where TMessage : IMessage
{
    /// <summary>
    /// Asynchronously subscribe and handle a message.
    /// </summary>
    /// <param name="message">The message object</param>
    /// <param name="cancellationToken">An cancellation token</param>
    /// <returns>A Task</returns>
    /// <seealso cref="IMessage"/>
    ValueTask HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}