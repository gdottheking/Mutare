namespace Sharara.EntityCodeGen.Core
{
    class DateTimeField : Field
    {
        public const string XmlTypeName = "dt";

        public DateTimeField()
            : base(FieldType.DateTime.Instance)
        {
        }

        public override void Accept(IFieldVisitor visitor) {
            visitor.VisitDateTimeField(this);
        }
    }
}