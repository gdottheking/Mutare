namespace Sharara.EntityCodeGen.Core
{
    class ListField : Field
    {
        public const string XmlTypeName = "list";

        public ListField(FieldType itemType, string name)
            : base(new FieldType.List(itemType), name)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitListField(this);
        }
    }
}