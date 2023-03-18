namespace Sharara.EntityCodeGen.Core
{
    class Argument<T> : IArgument<T>
    {
        public Argument(T Value, string? Name)
        {
            this.ArgType = Value;
            this.Name = Name;
        }

        public Argument(T value) : this(value, null)
        {
        }

        public T ArgType { get; set; }

        public string? Name { get; set; }

        object IArgument.ArgType => ArgType;
    }
}