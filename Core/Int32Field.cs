namespace Sharara.EntityCodeGen.Core
{
    class Int32Field : Field
    {
        public const string XmlTypeName = "i32";
        public long MinValue { get; set; } = long.MinValue;
        public long MaxValue { get; set; } = long.MaxValue;

        public Int32Field(string name)
            : base(FieldType.Int32.Instance, name)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitInt32Field(this);
        }
    }
}