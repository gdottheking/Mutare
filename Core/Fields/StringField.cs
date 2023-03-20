namespace Sharara.EntityCodeGen.Core.Fields
{
    [Flags]
    enum Transform
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
        public long MinLength { get; set; }
        public long MaxLength { get; set; } = long.MaxValue;
        public string? RegexPattern { get; set; }
        public Transform Transforms { get; set; } = Transform.Trim;
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