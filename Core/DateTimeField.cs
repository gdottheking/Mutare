namespace Sharara.EntityCodeGen.Core
{
    class DateTimeField : Field
    {
        public const string XmlTypeName = "dt";

        public DateTimeField()
            : base(FieldType.DateTime)
        {
        }

        public override void Accept(IFieldVisitor visitor) {
            visitor.VisitDateTimeField(this);
        }
    }
}