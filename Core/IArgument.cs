namespace Sharara.EntityCodeGen.Core
{
    interface IArgument
    {
        object? ArgType { get; }
        string? Name { get; }
    }

    interface IArgument<out T> : IArgument
    {
        new T ArgType { get; }
    }

}