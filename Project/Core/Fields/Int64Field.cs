namespace Sharara.EntityCodeGen.Core.Fields
{
    class Int64Field : Field
    {
        public const string XmlTypeName = "i64";
        public long MinValue { get; set; } = long.MinValue;
        public long MaxValue { get; set; } = long.MaxValue;

        public Int64Field(RecordEntity record, string name)
            : base(record, FieldType.Int64.Instance, name)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitInt64Field(this);
        }
    }
}