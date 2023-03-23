namespace Sharara.EntityCodeGen.Core.Fields.Types
{
    abstract partial class FieldType
    {
        public class Int64 : FieldType
        {
            public Int64() : base(FieldTypeId.Int64)
            {
            }
            public static readonly Int64 Instance = new Int64();
            public string ClrType { get; } = "long";
            public override string GrpcType { get; } = "int64";
        }
    }
}