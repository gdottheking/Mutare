using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Core
{
    class RecordEntity : Entity
    {
        public const string XmlTypeName = "record";

        public readonly List<Field> fields = new List<Field>();

        public RecordEntity(string name, string pluralName)
            : base(name, pluralName)
        {
        }

        public IEnumerable<Field> Keys() => fields.Where(f => f.IsKey);

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.VisitRecord(this);
        }

        public override void Validate()
        {
            base.Validate();


            foreach (var field in fields)
            {
                if (!fieldNameRex.IsMatch(field.Name))
                {
                    throw new InvalidOperationException($"FieldName {Name}.{field.Name} must be in PascalCase");
                }
            }

            // Duplicates will throw
            try
            {
                fields.ToDictionary(v => v.Name, v => v);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Entity {Name}. Field has duplicate name {e.Message}");
            }

            try
            {
                fields.ToDictionary(v => v.Name, v => v);
                fields.ToDictionary(v => v.ProtoId, v => v);
            }
            catch (ArgumentException e)
            {
                throw new Exception($"Entity {Name}. Field has duplicate proto id: {e.Message}");
            }
        }
    }
}