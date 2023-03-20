namespace Sharara.EntityCodeGen.Core.Fields
{
    class Float64Field : Field
    {
        public const string XmlTypeName = "f64";
        public long MinValue { get; set; } = long.MinValue;
        public long MaxValue { get; set; } = long.MaxValue;

        public Float64Field(string name)
            : base(FieldType.Float64.Instance, name)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitFloat64Field(this);
        }
    }
}