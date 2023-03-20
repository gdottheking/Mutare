namespace Sharara.EntityCodeGen.Core.Fields
{
    class ReferenceField : Field
    {
        public const string XmlTypeName = "ref";

        public ReferenceField(FieldType.EntityNameRef nameRef, string name)
            : base(nameRef, name)
        {
        }

        public ReferenceField(FieldType.EntityRef etyRef, string name)
            : base(etyRef, name)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitReferenceField(this);
        }
    }
}