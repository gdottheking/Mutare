namespace Sharara.EntityCodeGen.Core.Fields.Types
{
    abstract partial class FieldType
    {
        public class Void : FieldType
        {
            public static readonly Void Instance = new Void();

            public Void() : base(FieldTypeId.Void)
            {
            }

            public string ClrType { get; } = "void";

            public override string GrpcType => throw new InvalidOperationException("Void type cannot be mapped to Grpc");
        }
    }
}