namespace Sharara.EntityCodeGen.Core
{
    class RecordEntity : Entity
    {
        public const string XmlTypeName = "record";

        public readonly List<Field> fields = new List<Field>();

        public IEnumerable<Field> Keys() => fields.Where(f => f.IsKey);

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.VisitRecord(this);
        }
    }
}