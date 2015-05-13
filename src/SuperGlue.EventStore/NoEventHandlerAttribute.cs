using System;

namespace SuperGlue.EventStore
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class NoEventHandlerAttribute : Attribute
    {
    }
}