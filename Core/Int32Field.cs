namespace Sharara.EntityCodeGen.Core
{
    class Int32Field : Field
    {
        public const string XmlTypeName = "i32";
        public long MinValue { get; set; } = long.MinValue;
        public long MaxValue { get; set; } = long.MaxValue;

        public Int32Field()
            : base(FieldType.Int32.Instance)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitInt32Field(this);
        }
    }
}