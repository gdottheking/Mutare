namespace Sharara.EntityCodeGen.Core.Fields
{
    class DateTimeField : Field
    {
        public const string XmlTypeName = "date";

        public DateTimeField(RecordEntity record, string name)
            : base(record, FieldType.DateTime.Instance, name)
        {
        }

        public override void Accept(IFieldVisitor visitor) {
            visitor.VisitDateTimeField(this);
        }
    }
}