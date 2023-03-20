namespace Sharara.EntityCodeGen.Core.Fields
{
    class DateTimeField : Field
    {
        public const string XmlTypeName = "dt";

        public DateTimeField(string name)
            : base(FieldType.DateTime.Instance, name)
        {
        }

        public override void Accept(IFieldVisitor visitor) {
            visitor.VisitDateTimeField(this);
        }
    }
}