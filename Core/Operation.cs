namespace Sharara.EntityCodeGen.Core
{
    record Operation(Entity entity,
                     OperationType OperationType,
                     params IArgument[] Arguments)
    {
        public string Name => $"{OperationType}{entity.Name}";

        public Object ReturnType
        {
            get
            {
                return OperationType switch
                {
                    OperationType.Count => FieldType.Int64,
                    OperationType.Delete => FieldType.None,
                    OperationType.Get => entity,
                    OperationType.List => new Many(entity),
                    OperationType.Put => FieldType.None,
                    _ => throw new NotImplementedException()
                };
            }
        }

        public record Many(Object ItemType);
    }
}