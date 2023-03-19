namespace Sharara.EntityCodeGen.Core
{
    class ReferenceField : Field
    {
        public const string XmlTypeName = "ref";

        public ReferenceField(FieldType.EntityNameRef nameRef)
            : base(nameRef)
        {
        }

        public ReferenceField(FieldType.EntityRef etyRef)
            : base(etyRef)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitReferenceField(this);
        }
    }
}