namespace Sharara.EntityCodeGen.Core.Fields
{
    class ListField : Field
    {
        public const string XmlTypeName = "list";

        public ListField(RecordEntity record, FieldType itemType, string name)
            : base(record, new FieldType.List(itemType), name)
        {
        }

        internal new FieldType.List FieldType => (FieldType.List)base.FieldType;

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitListField(this);
        }
    }
}