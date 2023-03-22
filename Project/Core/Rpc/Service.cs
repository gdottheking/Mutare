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
                // else if (e is EnumEntity en)
                // {
                //     AddProcedures(en.BackingRecord__Hack());
                // }
            }
        }

        private void AddProcedures(RecordEntity record)
        {
            var pkArgs = record.GetKeyFields().Select(
                k => new Argument(k.FieldType, k.Name.ToCamelCase())
            ).ToArray();

            operations.Add(new Procedure(record, ProcedureType.Count));
            operations.Add(new Procedure(record, ProcedureType.Delete, pkArgs));
            operations.Add(new Procedure(record, ProcedureType.Get, pkArgs));
            operations.Add(
                new Procedure(record, ProcedureType.List,
                    new Argument(FieldType.Int32.Instance, "page"),
                    new Argument(FieldType.Int32.Instance, "count")
                )
            );
            operations.Add(
                new Procedure(record,
                    ProcedureType.Put,
                    new Argument(new FieldType.Entity(record), record.Name!.ToCamelCase())
                )
            );
            operations.Add(
                new Procedure(record,
                    ProcedureType.Update,
                    new Argument(new FieldType.Entity(record), record.Name!.ToCamelCase())
                )
            );
        }

        record Procedure(RecordEntity Record, ProcedureType ProcedureType, params Argument[] Arguments)
            : IProcedure
        {
            public string Name => ProcedureType switch
            {
                ProcedureType.List => $"GetAll{Record.PluralName}",
                ProcedureType.Count => $"Get{Record.Name}Count",
                _ => $"{ProcedureType}{Record.Name}"
            };

            public FieldType ReturnType
            {
                get
                {
                    return ProcedureType switch
                    {
                        ProcedureType.List => new FieldType.List(new FieldType.Entity(Record)),
                        ProcedureType.Get => new FieldType.Entity(Record),
                        ProcedureType.Count => FieldType.Int64.Instance,
                        ProcedureType.Delete => FieldType.Void.Instance,
                        ProcedureType.Put => FieldType.Void.Instance,
                        ProcedureType.Update => FieldType.Void.Instance,
                        _ => throw new NotImplementedException()
                    };
                }
            }
        }

    }
}