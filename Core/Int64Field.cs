namespace Sharara.EntityCodeGen.Core
{
    class Int64Field : Field
    {
        public const string XmlTypeName = "i64";
        public long MinValue { get; set; } = long.MinValue;
        public long MaxValue { get; set; } = long.MaxValue;

        public Int64Field()
            : base(FieldType.Int64)
        {
        }

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.VisitInt64Field(this);
        }
    }
}