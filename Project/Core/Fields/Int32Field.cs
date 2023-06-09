namespace Sharara.EntityCodeGen.Core.Fields
{
    record class Int32Field : Field
    {
        public const string XmlTypeName = "i32";
        public long MinValue { get; set; } = long.MinValue;
        public long MaxValue { get; set; } = long.MaxValue;

        public Int32Field(RecordEntity record, string name)
            : base(record, int32Type(), name)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitInt32Field(this);
        }
    }
}