namespace Sharara.EntityCodeGen.Core.Fields
{
    record class Float64Field : Field
    {
        public const string XmlTypeName = "f64";
        public double MinValue { get; set; } = double.MinValue;
        public double MaxValue { get; set; } = double.MaxValue;

        public Float64Field(RecordEntity record, string name)
            : base(record, float64Type(), name)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitFloat64Field(this);
        }
    }
}