namespace Sharara.EntityCodeGen.Core
{
    class Service
    {
        private readonly Schema schema;

        private readonly List<Operation> operations = new List<Operation>();

        internal ICollection<Operation> Operations => operations.AsReadOnly();

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
                k => new FieldTypeArgument(k.FieldType, k.Name)
            ).ToArray();

            operations.Add(new Operation(record, OperationType.Count));
            operations.Add(new Operation(record, OperationType.Delete, pkArgs));
            operations.Add(new Operation(record, OperationType.Get, pkArgs));
            operations.Add(new Operation(record, OperationType.List,
                new FieldTypeArgument(FieldType.Int64, "pageIndex"),
                new FieldTypeArgument(FieldType.Int64, "pageSize")
                ));
            operations.Add(new Operation(record, OperationType.Put, new EntityArgument(record)));
        }

    }
}