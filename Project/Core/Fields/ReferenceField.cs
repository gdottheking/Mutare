namespace Sharara.EntityCodeGen.Core.Fields
{
    class ReferenceField : Field
    {
        public const string XmlTypeName = "ref";

        public ReferenceField(FieldType.Entity entity, string name)
            : base(entity, name)
        {
        }

        public new FieldType.Entity FieldType => (FieldType.Entity) base.FieldType;

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitReferenceField(this);
        }
    }
}