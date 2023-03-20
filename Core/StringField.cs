namespace Sharara.EntityCodeGen.Core
{
    class StringField : Field
    {
        public const string XmlTypeName = "s";

        public long MinLength { get; set; }
        public long MaxLength { get; set; } = long.MaxValue;

        public StringField(string name)
            : base(FieldType.String.Instance, name)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitStringField(this);
        }
    }
}