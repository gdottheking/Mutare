using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Core
{
    class RecordEntity : Entity
    {
        public const string XmlTypeName = "record";

        public RecordEntity(string name, string pluralName)
            : base(name, pluralName, EntityType.Record)
        {
        }

        public List<Field> Fields { get; } = new List<Field>();

        public Field? this[string name]
        {
            get => Fields.Where(f => 0 == string.Compare(name, f.Name, true)).FirstOrDefault();
        }

        public IEnumerable<Field> GetKeyFields() => Fields.Where(f => f.IsKey);

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.VisitRecord(this);
        }

        public ICollection<ShadowField> ShadowFields()
        {
            // Get all reference fields which point to a Record
            var relationFields = Fields.Where(f => f is ReferenceField rf &&
                    rf.FieldType.GetEntity().EntityType == EntityType.Record)
                    .Cast<ReferenceField>();

            List<ShadowField> shadowFields = new List<ShadowField>();

            // Create a shadow field for each Key on the relation
            foreach (var recField in relationFields)
            {
                var otherRecord = (RecordEntity)recField.FieldType.GetEntity();
                foreach (var pkField in otherRecord.GetKeyFields())
                {
                    var y = new FieldOwnership(otherRecord, pkField);
                    var shadow = new ShadowField(recField, y);
                    shadowFields.Add(shadow);
                }
            }

            return shadowFields;
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

    record FieldOwnership(RecordEntity Owner, Field Field)
    {
        public override string ToString()
        {
            return $"{Owner.Name}.{Field.Name}";
        }
    }

    record ShadowField(Field Field, FieldOwnership Target)
    {
        public string Name => Field.Name + Target.Field.Name;
        public override string ToString() => Name;
    }
}