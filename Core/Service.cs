namespace Sharara.EntityCodeGen.Core
{
    class Service
    {
        private readonly Schema schema;

        private readonly List<OperationInfo> operations = new List<OperationInfo>();

        internal ICollection<OperationInfo> Operations => operations.AsReadOnly();

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

            operations.Add(new OperationInfo(record, OperationType.Count));
            operations.Add(new OperationInfo(record, OperationType.Delete, pkArgs));
            operations.Add(new OperationInfo(record, OperationType.Get, pkArgs));
            operations.Add(new OperationInfo(record, OperationType.List,
                new FieldTypeArgument(FieldType.Int64, "pageIndex"),
                new FieldTypeArgument(FieldType.Int64, "pageSize")
                ));
            operations.Add(new OperationInfo(record, OperationType.Put, new EntityArgument(record)));
        }

    }
}