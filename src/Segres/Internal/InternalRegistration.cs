﻿using System.Reflection;
using Segres.Contracts;
using Segres.Handlers;
using Segres.Internal.Cache;

namespace Segres.Internal;

internal static class InternalRegistration
{
    public static HandlerCache<HandlerInfo> GetCommandHandlerDetails(this ReadOnlySpan<Assembly> assemblies)
    {
        var dic = new HandlerCache<HandlerInfo>();

        foreach (var requestHandlerDetail in assemblies.GetCommandRequestHandlerDetails_2())
            dic.Add(requestHandlerDetail);

        foreach (var requestHandlerDetail in assemblies.GetCommandRequestHandlerDetails_1())
            dic.Add(requestHandlerDetail);

        return dic;
    }    
    
    public static HandlerCache<HandlerInfo> GetQueryHandlerDetails(this ReadOnlySpan<Assembly> assemblies)
    {
        return assemblies.GetQueryRequestHandlerDetails_2();
    }    
    
    public static HandlerCache<HandlerInfo> GetStreamHandlerDetails(this ReadOnlySpan<Assembly> assemblies)
    {
        return assemblies.GetHandlerDetails(typeof(IStreamHandler<,>))
            .ToHandlerCache((streamType, handlerType) =>
            {
                var resultType = streamType.GetInterface(typeof(IStream<>).Name)!.GetGenericArguments()[0];

                var method = typeof(Delegates).GetMethod(nameof(Delegates.CreateStreamDelegate));
                var del = (Delegate) method!.MakeGenericMethod(resultType).Invoke(null, new object?[] {streamType})!;

                return new HandlerInfo(handlerType, del);
            });
    }

    private static HandlerCache<HandlerInfo> GetQueryRequestHandlerDetails_2(this ReadOnlySpan<Assembly> assemblies)
    {
        return assemblies.GetHandlerDetails(typeof(IQueryHandler<,>))
            .ToHandlerCache((requestType, handlerType) =>
            {
                var responseType = requestType.GetInterface(typeof(IQuery<>).Name)!.GetGenericArguments()[0];

                var method = typeof(Delegates).GetMethod(nameof(Delegates.CreateQueryDelegate));
                var del = (Delegate) method!.MakeGenericMethod(responseType).Invoke(null, new object?[] {requestType})!;

                return new HandlerInfo(handlerType, del);
            });
    }

    private static HandlerCache<HandlerInfo> GetCommandRequestHandlerDetails_1(this ReadOnlySpan<Assembly> assemblies)
    {
        return assemblies.GetHandlerDetails(typeof(ICommandHandler<>))
            .ToHandlerCache((requestType, handlerType) =>
            {
                var method = typeof(Delegates).GetMethod(nameof(Delegates.CreateCommandWithoutResponseDelegate))!;
                var del = (Delegate) method.Invoke(null, new object?[] {requestType})!;

                return new HandlerInfo(handlerType, del);
            });
    }

    private static HandlerCache<HandlerInfo> GetCommandRequestHandlerDetails_2(this ReadOnlySpan<Assembly> assemblies)
    {
        return assemblies.GetHandlerDetails(typeof(ICommandHandler<,>))
            .ToHandlerCache((requestType, handlerType) =>
            {
                var responseType = requestType.GetInterface(typeof(ICommand<>).Name)!.GetGenericArguments()[0];

                var method = typeof(Delegates).GetMethod(nameof(Delegates.CreateCommandDelegate));
                var del = (Delegate) method!.MakeGenericMethod(responseType).Invoke(null, new object?[] {requestType})!;

                return new HandlerInfo(handlerType, del);
            });
    }


    internal static IEnumerable<KeyValuePair<Type, Type>> GetHandlerDetails(this ReadOnlySpan<Assembly> assemblies, Type type)
    {
        var handlerDetails = new List<KeyValuePair<Type, Type>>();

        foreach (var assembly in assemblies)
        {
            var handlers = GetHandlerDetails(assembly, type);
            handlerDetails.AddRange(handlers);
        }

        return handlerDetails;
    }

    private static IEnumerable<KeyValuePair<Type, Type>> GetHandlerDetails(this Assembly assembly, Type type)
    {
        var classesImplementingInterface = GetClassesImplementingInterface(assembly, type);

        var details = new List<KeyValuePair<Type, Type>>();

        foreach (var value in classesImplementingInterface)
        {
            var types = value
                .GetInterfaces()
                .Where(x => x.Name == type.Name)
                .Select(x => x.GetGenericArguments()[0]);

            foreach (var key in types)
            {
                details.Add(new KeyValuePair<Type, Type>(key, value));
            }
        }

        return details;
    }

    private static IEnumerable<Type> GetClassesImplementingInterface(this Assembly assembly, Type typeToMatch)
    {
        return assembly.DefinedTypes.Where(type =>
        {
            var isImplementRequestType = type
                .GetInterfaces()
                .Where(x => x.IsGenericType)
                .Any(x => x.GetGenericTypeDefinition() == typeToMatch);

            return !type.IsInterface && !type.IsAbstract && isImplementRequestType;
        }).ToList();
    }
}