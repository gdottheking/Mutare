using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Core
{
    class EnumEntity : Entity
    {
        public const string XmlTypeName = "enum";
        public readonly List<EnumValue> Values = new List<EnumValue>();
        //private RecordEntity? backingRecord;

        public EnumEntity(string name, string pluralName)
            : base(name, pluralName, EntityType.Enum)
        {
        }

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.VisitEnum(this);
        }

        public override void Validate()
        {
            base.Validate();

            foreach (var enumValue in Values)
            {
                if (!Common.PascalCaseRegex.IsMatch(enumValue.Name))
                {
                    throw new InvalidOperationException($"Enum Value {Name}.{enumValue.Name} must be in PascalCase");
                }
            }

            // Duplicates should throw
            try
            {
                Values.ToDictionary(v => v.Name, v => v);
                Values.ToDictionary(v => v.Value, v => v);
            }
            catch (ArgumentException e)
            {
                throw new Exception($"Enum entity {Name} is invalid: {e.Message}");
            }
        }

        // internal RecordEntity BackingRecord__Hack()
        // {
        //     // THIS IS A HACK
        //     if (backingRecord == null)
        //     {
        //         backingRecord = new RecordEntity(Name, PluralName);
        //         backingRecord.Fields.Add(new Int32Field("Id") { IsKey = true, IsRequired = true });
        //         backingRecord.Fields.Add(new StringField("Display") { MinLength = 0, MaxLength = 512 });
        //         backingRecord.Fields.Add(new StringField("Description") { MinLength = 0, MaxLength = 512 });
        //     }
        //     return backingRecord;
        // }
    }
}