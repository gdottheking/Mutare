namespace Sharara.EntityCodeGen.Core.Fields.Types
{
    abstract partial class FieldType
    {
        public class Entity : FieldType
        {
            private Core.Entity? resolvedEntity;
            private string? unresolvedEntityName;

            public Entity(Core.Entity e) : base(FieldTypeId.Entity)
            {
                this.resolvedEntity = e;
            }

            public Entity(string name) : base(FieldTypeId.Entity)
            {
                this.unresolvedEntityName = name;
            }

            public string? UnresolvedEntityName => unresolvedEntityName;

            public bool ResolvesToEntity => resolvedEntity != null;

            public Core.Entity GetEntity()
            {
                if (resolvedEntity == null)
                {
                    throw new InvalidOperationException("Unresolved entity: " + UnresolvedEntityName);
                }
                return resolvedEntity;
            }

            public void ResolveTo(Core.Entity entity)
            {
                resolvedEntity = entity;
            }

            public override string GrpcType
            {
                get
                {
                    return resolvedEntity?.Name ??
                        throw new InvalidOperationException("Unresolved entity " + UnresolvedEntityName);
                }
            }
        }
    }
}