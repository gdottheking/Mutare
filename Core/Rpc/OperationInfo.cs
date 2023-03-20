
using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Core.Rpc
{
    record OperationInfo(Entity Entity,
                     OperationType OperationType,
                     params Argument[] Arguments)
    {
        public string Name => $"{OperationType}{Entity.Name}";

        public FieldType ReturnType
        {
            get
            {
                return OperationType switch
                {
                    OperationType.List => new FieldType.List(new FieldType.EntityRef(Entity)), // HACK
                    OperationType.Get => new FieldType.EntityRef(Entity),
                    OperationType.Count => FieldType.Int64.Instance,
                    OperationType.Delete => FieldType.Void.Instance,
                    OperationType.Put => FieldType.Void.Instance,
                    _ => throw new NotImplementedException()
                };
            }
        }
    }

}