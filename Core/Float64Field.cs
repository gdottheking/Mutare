namespace Sharara.EntityCodeGen.Core
{
    class Float64Field : Field
    {
        public const string XmlTypeName = "f64";
        public long MinValue { get; set; } = long.MinValue;
        public long MaxValue { get; set; } = long.MaxValue;

        public Float64Field()
            : base(FieldType.Float64)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitFloat64Field(this);
        }
    }
}