namespace Sharara.EntityCodeGen.Core.Fields
{
    record class DateTimeField : Field
    {
        public const string XmlTypeName = "date";

        public DateTimeField(RecordEntity record, string name)
            : base(record, dateTimeType(), name)
        {
        }

        public override void Accept(IFieldVisitor visitor) {
            visitor.VisitDateTimeField(this);
        }
    }
}