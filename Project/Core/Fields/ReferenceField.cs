namespace Sharara.EntityCodeGen.Core.Fields
{
    class ReferenceField : Field
    {
        public const string XmlTypeName = "ref";

        public ReferenceField(RecordEntity record, FieldType.Entity entity, string name)
            : base(record, entity, name)
        {
        }

        public new FieldType.Entity FieldType => (FieldType.Entity) base.FieldType;

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitReferenceField(this);
        }
    }
}