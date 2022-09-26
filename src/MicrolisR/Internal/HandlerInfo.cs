﻿namespace MicrolisR;

internal sealed class HandlerInfo
{
    private readonly Delegate _method;
    
    public HandlerInfo(Type type, Delegate method)
    {
        Type = type;
        this._method = method;
    }

    public Type Type { get; }
    

    
    public TDelegate ResolveMethod<TDelegate>()
        where TDelegate : Delegate => (TDelegate)_method;
}