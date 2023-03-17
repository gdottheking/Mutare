namespace Sharara.EntityCodeGen.Core
{
    class EnumEntity : Entity
    {
        public const string XmlTypeName = "enum";
        public readonly List<EnumValue> Values = new List<EnumValue>();

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.VisitEnum(this);
        }
    }
}