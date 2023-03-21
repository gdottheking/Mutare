using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Core
{
    class RecordEntity : Entity
    {
        public const string XmlTypeName = "record";

        public RecordEntity(string name, string pluralName)
            : base(name, pluralName)
        {
        }

        public List<Field> Fields { get; } = new List<Field>();

        public Field? this[string name]
        {
            get => Fields.Where(f => 0 == string.Compare(name, f.Name, true)).FirstOrDefault();
        }

        public IEnumerable<Field> Keys() => Fields.Where(f => f.IsKey);

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.VisitRecord(this);
        }

        public override void Validate()
        {
            base.Validate();

            foreach (var field in Fields)
            {
                if (!Common.PascalCaseRegex.IsMatch(field.Name))
                {
                    throw new InvalidOperationException($"FieldName {Name}.{field.Name} must be in PascalCase");
                }
            }

            // Duplicates will throw
            try
            {
                Fields.ToDictionary(v => v.Name, v => v);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Entity {Name}. Field has duplicate name {e.Message}");
            }

            try
            {
                Fields.ToDictionary(v => v.Name, v => v);
                Fields.ToDictionary(v => v.ProtoId, v => v);
            }
            catch (ArgumentException e)
            {
                throw new Exception($"Entity {Name}. Field has duplicate proto id: {e.Message}");
            }
        }
    }
}