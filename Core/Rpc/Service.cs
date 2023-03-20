using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Core.Rpc
{
    class Service
    {
        private readonly Schema schema;

        private readonly List<IProcedure> operations = new List<IProcedure>();

        internal ICollection<IProcedure> Procedures => operations.AsReadOnly();

        internal Schema Schema => schema;

        public Service(Schema schema)
        {
            this.schema = schema;
            Build();
        }

        private void Build()
        {
            foreach (Entity e in Schema.Entities)
            {
                if (e is RecordEntity r)
                {
                    AddOperations(r);
                }
                else if (e is EnumEntity en)
                {
                    AddOperations(en.BackingRecord__Hack());
                }
            }
        }


        private void AddOperations(RecordEntity record)
        {
            var pkArgs = record.Keys().Select(
                k => new Argument(k.FieldType, k.Name)
            ).ToArray();

            operations.Add(new Procedure(record, OperationType.Count));
            operations.Add(new Procedure(record, OperationType.Delete, pkArgs));
            operations.Add(new Procedure(record, OperationType.Get, pkArgs));
            operations.Add(
                new Procedure(record, OperationType.List,
                    new Argument(FieldType.Int64.Instance, "page"),
                    new Argument(FieldType.Int64.Instance, "count")
                )
            );
            operations.Add(
                new Procedure(record,
                    OperationType.Put,
                    new Argument(new FieldType.EntityRef(record), record.Name!.ToLower())
                )
            );
        }



        record Procedure(Entity Entity, OperationType ProcedureType, params Argument[] Arguments)
            : IProcedure
        {
            public string Name => ProcedureType switch
            {
                OperationType.List => $"GetAll{Entity.PluralName}",
                OperationType.Count => $"Get{Entity.PluralName}Count",
                _ => $"{ProcedureType}{Entity.Name}"
            };

            public FieldType ReturnType
            {
                get
                {
                    return ProcedureType switch
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
}