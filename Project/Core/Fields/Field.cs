using Sharara.EntityCodeGen.Core.Fields.Types;

namespace Sharara.EntityCodeGen.Core.Fields
{
    record class Field
    {
        public RecordEntity Record { get; }
        public FieldType FieldType { get; }
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public bool IsKey { get; set; }
        public bool IsNullable => !IsKey && !IsRequired;
        public bool CheckOnUpdate { get; set; }
        public int ProtoId { get; set; }

        protected Field(RecordEntity record, FieldType type, string name)
        {
            this.Record = record;
            FieldType = type;
            Name = name;
        }

        public virtual void Accept(IFieldVisitor visitor)
        {
            visitor.VisitField(this);
        }

        public override string ToString()
        {
            return (IsRequired ? "*" : "") + $"{Name}:{FieldType}";
        }

        public static FieldType int32Type() => FieldType.Int32.Instance;

        public static FieldType int64Type() => FieldType.Int64.Instance;

        public static FieldType float64Type() => FieldType.Float64.Instance;

        public static FieldType stringType() => FieldType.String.Instance;

        public static FieldType dateTimeType() => FieldType.DateTime.Instance;

        public static FieldType entityType(Entity e) => new FieldType.Entity(e);

        public static FieldType listType(FieldType itemType) => new FieldType.List(itemType);

    }
}