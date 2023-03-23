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
            var pointerFields = Fields.Where(f => f is ReferenceField rf &&
                    rf.FieldType.GetEntity().EntityType == EntityType.Record)
                    .Cast<ReferenceField>();

            List<ShadowField> shadowFields = new List<ShadowField>();

            // Create a shadow field for each Key on the relation
            foreach (var pointerField in pointerFields)
            {
                var targetRecord = (RecordEntity)pointerField.FieldType.GetEntity();
                foreach (var targetRecPkField in targetRecord.GetKeyFields())
                {
                    var shadow = new ShadowField(pointerField, targetRecPkField);
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

            // Validate that field names are unique
            try
            {
                var dict = Fields.ToDictionary(field => field.Name, field => field.Name);
                foreach (var shadow in ShadowFields())
                {
                    dict.Add(shadow.Name, shadow.Name);
                }
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Entity {Name}. Field has duplicate field names {e.Message}");
            }

            // Validate that protobuf ids are unique
            try
            {
                Fields.ToDictionary(field => field.ProtoId, field => field.ProtoId);
            }
            catch (ArgumentException e)
            {
                throw new Exception($"Entity {Name}. Field has duplicate proto id: {e.Message}");
            }
        }
    }

}