namespace Sharara.EntityCodeGen.Core
{
    enum EntityType
    {
        None,
        Record,
        Enum
    }

    abstract class Entity
    {
        public string Name { get; private set; }
        public string PluralName { get; private set; }
        public EntityType EntityType { get; private set; }

        protected Entity(string name, string pluralName, EntityType entityType)
        {
            this.Name = name;
            this.PluralName = pluralName;
            this.EntityType = entityType;
        }

        public virtual void Accept(IEntityVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public virtual void Validate()
        {
            if (!Common.PascalCaseRegex.IsMatch(Name))
            {
                throw new InvalidOperationException($"Entity name '{Name}' must be in PascalCase");
            }

            if (!Common.PascalCaseRegex.IsMatch(PluralName))
            {
                throw new InvalidOperationException($"Entity name '{PluralName}' must be in PascalCase");
            }
        }


        public override string ToString()
        {
            return $"{this.GetType().Name}({Name})";
        }

    }
}