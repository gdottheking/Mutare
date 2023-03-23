namespace Sharara.EntityCodeGen.Core.Fields
{
    record class Int64Field : Field
    {
        public const string XmlTypeName = "i64";
        public long MinValue { get; set; } = long.MinValue;
        public long MaxValue { get; set; } = long.MaxValue;

        public Int64Field(RecordEntity record, string name)
            : base(record, int64Type(), name)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitInt64Field(this);
        }
    }
}