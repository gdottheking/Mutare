namespace Sharara.EntityCodeGen.Core
{
    class ListField : Field
    {
        public const string XmlTypeName = "list";

        public ListField(FieldType itemType)
            : base(new FieldType.List(itemType))
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitListField(this);
        }
    }
}