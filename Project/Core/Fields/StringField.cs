namespace Sharara.EntityCodeGen.Core.Fields
{
    [Flags]
    enum StringTransform
    {
        None = 0,
        TrimStart = 1,
        TrimEnd = 2,
        Trim = 3,
        ToUpper = 4,
        ToLower = 8
    }

    class StringField : Field
    {
        public const string XmlTypeName = "str";
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? RegexPattern { get; set; }
        public StringTransform Transforms { get; set; } = StringTransform.Trim;
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