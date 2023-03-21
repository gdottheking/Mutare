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
                    AddProcedures(r);
                }
                else if (e is EnumEntity en)
                {
                    AddProcedures(en.BackingRecord__Hack());
                }
            }
        }

        private void AddProcedures(RecordEntity record)
        {
            var pkArgs = record.Keys().Select(
                k => new Argument(k.FieldType, k.Name.ToCamelCase())
            ).ToArray();

            operations.Add(new Procedure(record, OperationType.Count));
            operations.Add(new Procedure(record, OperationType.Delete, pkArgs));
            operations.Add(new Procedure(record, OperationType.Get, pkArgs));
            operations.Add(
                new Procedure(record, OperationType.List,
                    new Argument(FieldType.Int32.Instance, "page"),
                    new Argument(FieldType.Int32.Instance, "count")
                )
            );
            operations.Add(
                new Procedure(record,
                    OperationType.Put,
                    new Argument(new FieldType.EntityRef(record), record.Name!.ToCamelCase())
                )
            );
        }

        record Procedure(RecordEntity Record, OperationType ProcedureType, params Argument[] Arguments)
            : IProcedure
        {
            public string Name => ProcedureType switch
            {
                OperationType.List => $"GetAll{Record.PluralName}",
                OperationType.Count => $"Get{Record.Name}Count",
                _ => $"{ProcedureType}{Record.Name}"
            };

            public FieldType ReturnType
            {
                get
                {
                    return ProcedureType switch
                    {
                        OperationType.List => new FieldType.List(new FieldType.EntityRef(Record)), // HACK
                        OperationType.Get => new FieldType.EntityRef(Record),
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