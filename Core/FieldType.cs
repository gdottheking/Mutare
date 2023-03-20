namespace Sharara.EntityCodeGen.Core
{
    class FieldType
    {
        public class Void : FieldType
        {
            public static readonly Void Instance = new Void();
        }

        public class String : FieldType
        {
            public static readonly String Instance = new String();
        }

        public class Int64 : FieldType
        {
            public static readonly Int64 Instance = new Int64();
        }

        public class Int32 : FieldType
        {
            public static readonly Int32 Instance = new Int32();
        }

        public class Float64 : FieldType
        {
            public static readonly Float64 Instance = new Float64();
        }

        public class DateTime : FieldType
        {
            public static readonly DateTime Instance = new DateTime();
        }

        public class EntityRef : FieldType
        {
            public EntityRef(Entity e)
            {
                this.Entity = e;
            }

            public Entity Entity { get; }
        }

        public class EntityNameRef : FieldType
        {
            public EntityNameRef(string name)
            {
                this.EntityName = name;
            }

            public string EntityName { get; }
            public Entity? ResolvedEntity { get; private set; }

            public void ResolveTo(Entity entity)
            {
                ResolvedEntity = entity;
            }
        }

        public class List : FieldType
        {
            public List(FieldType itemType)
            {
                ItemType = itemType;
            }
            public FieldType ItemType { get; }
        }
    }
}