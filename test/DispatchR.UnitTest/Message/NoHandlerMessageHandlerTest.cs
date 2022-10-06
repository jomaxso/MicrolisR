﻿using DispatchR.Extensions.DependencyInjection.Microsoft;
using DispatchR.UnitTest.Command;
using DispatchR.UnitTest.Event.Objects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Sdk;

namespace DispatchR.UnitTest.Message;

public class NoHandlerMessageHandlerTest
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
        .AddDispatchR(typeof(ResultCommand))
        .BuildServiceProvider();

    [Fact]
    public async Task PublishAsync_ShouldNotThrow_WhenMessageIsZeroCalled()
    {
        // Arrange
        var message = new NoHandlerMessage();
        var dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();
        
        // Act
        var task = async () => await dispatcher.PublishAsync(message);

        //Assert
        await task.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsyncWhenAll_ShouldNotThrow_WhenMessageIsZeroCalled()
    {
        // Arrange
        var message = new NoHandlerMessage();
        var dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();
        
        // Act
        var task = async () => await dispatcher.PublishAsync(message, Strategy.WhenAll);

        //Assert
        await task.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsyncWhenAny_ShouldNotThrow_WhenMessageIsZeroCalled()
    {
        // Arrange
        var message = new NoHandlerMessage();
        var dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();
        
        // Act
        var task = async () => await dispatcher.PublishAsync(message, Strategy.WhenAny);

        //Assert
        await task.Should().NotThrowAsync();
    }
}