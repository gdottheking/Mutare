namespace Sharara.EntityCodeGen.Core
{
    class StringField : Field
    {
        public const string XmlTypeName = "s";

        public long MinLength { get; set; }
        public long MaxLength { get; set; } = long.MaxValue;

        public StringField()
            : base(FieldType.String)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitStringField(this);
        }
    }
}