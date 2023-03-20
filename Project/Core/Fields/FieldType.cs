namespace Sharara.EntityCodeGen.Core.Fields
{
    abstract class FieldType
    {
        public abstract string GrpcType { get; }

        public class Void : FieldType
        {
            public static readonly Void Instance = new Void();

            public string ClrType { get; } = "void";

            public override string GrpcType => throw new InvalidOperationException("Void type cannot be mapped to Grpc");
        }

        public class String : FieldType
        {
            public static readonly String Instance = new String();
            public string ClrType { get; } = "string";
            public override string GrpcType { get; } = "string";
        }

        public class Int64 : FieldType
        {
            public static readonly Int64 Instance = new Int64();
            public string ClrType { get; } = "long";
            public override string GrpcType { get; } = "int64";
        }

        public class Int32 : FieldType
        {
            public static readonly Int32 Instance = new Int32();
            public string ClrType { get; } = "int";
            public override string GrpcType { get; } = "int32";
        }

        public class Float64 : FieldType
        {
            public static readonly Float64 Instance = new Float64();
            public string ClrType { get; } = "double";
            public override string GrpcType { get; } = "float64";
        }

        public class DateTime : FieldType
        {
            public static readonly DateTime Instance = new DateTime();
            public string ClrType { get; } = "DateTime";
            public override string GrpcType { get; } = "string";
        }

        public class EntityRef : FieldType
        {
            public EntityRef(Entity e)
            {
                this.Entity = e;
            }

            public Entity Entity { get; }
            public override string GrpcType
            {
                get => Entity.Name;
            }
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

            public override string GrpcType
            {
                get
                {
                    return ResolvedEntity?.Name ??
                        throw new InvalidOperationException("Unresolved entity " + EntityName);
                }
            }
        }

        public class List : FieldType
        {
            public List(FieldType itemType)
            {
                ItemType = itemType;
            }
            public FieldType ItemType { get; }

            public override string GrpcType
            {
                get => "repeated " + ItemType.GrpcType;

            }
        }
    }
}