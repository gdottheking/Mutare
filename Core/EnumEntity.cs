namespace Sharara.EntityCodeGen.Core
{
    class EnumEntity : Entity
    {
        public const string XmlTypeName = "enum";
        public readonly List<EnumValue> Values = new List<EnumValue>();
        private RecordEntity? backingRecord;

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.VisitEnum(this);
        }

        public override void Validate()
        {
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

        internal RecordEntity BackingRecord__Hack()
        {
            // THIS IS A HACK
            if (backingRecord == null)
            {
                backingRecord = new RecordEntity();
                backingRecord.Name = Name;
                backingRecord.fields.Add(new Int64Field { Name = "Id", IsKey = true, Required = true });
                backingRecord.fields.Add(new StringField { Name = "Display", MinLength = 0, MaxLength = 512 });
                backingRecord.fields.Add(new StringField { Name = "Description", MinLength = 0, MaxLength = 512 });
            }
            return backingRecord;
        }
    }
}