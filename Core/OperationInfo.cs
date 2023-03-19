namespace Sharara.EntityCodeGen.Core
{
    record OperationInfo(Entity Entity,
                     OperationType OperationType,
                     params IArgument[] Arguments)
    {
        public string Name => $"{OperationType}{Entity.Name}";

        public IReturn ReturnType
        {
            get
            {
                return OperationType switch
                {
                    OperationType.List => new Many(new EntityReturn(Entity)), // HACK
                    OperationType.Get => new EntityReturn(Entity),
                    OperationType.Count => new ScalarReturn(FieldType.Int64),
                    OperationType.Delete => new VoidReturn(),
                    OperationType.Put => new VoidReturn(),
                    _ => throw new NotImplementedException()
                };
            }
        }

        public interface IReturn { };
        public record Many(IReturn ItemType) : IReturn;
        public record EntityReturn(Entity Entity) : IReturn;
        public record ScalarReturn(FieldType FieldType) : IReturn;
        public record VoidReturn : IReturn;
    }

}