namespace Sharara.EntityCodeGen.Core
{
    class ReferenceField : Field
    {
        public const string XmlTypeName = "ref";
        public string EntityName { get; set; }


        public ReferenceField()
            : base(FieldType.Ref)
        {
        }

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.VisitReferenceField(this);
        }
    }
}