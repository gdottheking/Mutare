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

        /// List fields from other records, that reference this
        public List<ListField> IncomingPointers { get; } = new List<ListField>();

        // This record's fields which point to another record (NB: Could be self referencing)
        public IReadOnlyCollection<Field> OutgoingPointersScalar()
        {
            Func<Field, bool> PointsToARecord = (f) =>
            {
                bool isEntityType = f.FieldType.Id == Core.Fields.Types.FieldType.FieldTypeId.Entity;
                var entity = ((Core.Fields.Types.FieldType.Entity)f.FieldType).GetEntity();
                return entity.EntityType == EntityType.Record;
            };
            return Fields.Where(PointsToARecord).ToList().AsReadOnly();
        }

        public Field? this[string name]
        {
            get => Fields.Where(f => 0 == string.Compare(name, f.Name, true)).FirstOrDefault();
        }

        public IReadOnlyCollection<Field> PrimaryKeys() => Fields.Where(f => f.IsKey).ToList();

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.VisitRecord(this);
        }

        // TODO: Can't yet handle many-many relationships
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
                foreach (var targetRecPkField in targetRecord.PrimaryKeys())
                {
                    var shadow = new ShadowField(pointerField.Name + targetRecPkField.Name, pointerField, targetRecPkField);
                    shadowFields.Add(shadow);
                }
            }

            // This is a one-many relationship where 'this' Record is the many and incomingPointer.Record is 1
            // e.g. Business(1) -< Address (*)
            // Address needs a field named BusinessId (If Id is the primary key)
            // Business => incomingPointer.Record; Address => 'this'
            foreach (var incomingPointer in IncomingPointers)
            {
                foreach (var pk in incomingPointer.Record.PrimaryKeys())
                {
                    var inverse = pk with { IsRequiredOnCreate = false };
                    var shadow = new ShadowField(incomingPointer.Record.Name + pk.Name, incomingPointer, inverse);
                    shadowFields.Add(shadow);
                }

            }
            return shadowFields;
        }

        public override void Validate()
        {
            base.Validate();

            if (this.PrimaryKeys().Count() == 0)
            {
                throw new SchemaException($"The record {Name} does not have any primary keys");
            }

            var nameDict = new Dictionary<string, string>();
            var protoIdDict = new Dictionary<int, int>();
            foreach (var field in Fields)
            {
                if (!Common.PascalCaseRegex.IsMatch(field.Name))
                {
                    throw new SchemaException($"FieldName {Name}.{field.Name} must be in PascalCase");
                }

                // Validate that field names are unique
                try
                {
                    nameDict.Add(field.Name, field.Name);
                }
                catch (ArgumentException e)
                {
                    throw new SchemaException($"Entity {Name}. Field has duplicate field names", e);
                }

                // Validate that proto Ids names are unique
                try
                {
                    protoIdDict.Add(field.ProtoId, field.ProtoId);
                }
                catch (SchemaException e)
                {
                    throw new SchemaException($"Entity {Name}. Field has duplicate proto id", e);
                }

                if (field.FieldType.Id == Core.Fields.Types.FieldType.FieldTypeId.List)
                {
                    if (((ListField)field).FieldType.ItemType.GetEntity().EntityType != EntityType.Record)
                    {
                        throw new SchemaException($"List field {field.Record.Name}.{field.Name} does not reference a Record");
                    }
                }
            }
        }
    }

}