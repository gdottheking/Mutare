namespace Sharara.EntityCodeGen.Core.Fields.Types
{
    abstract partial class FieldType
    {
        public class Int32 : FieldType
        {
            public Int32() : base(FieldTypeId.Int32)
            {
            }
            public static readonly Int32 Instance = new Int32();
            public string ClrType { get; } = "int";
            public override string GrpcType { get; } = "int32";
        }
    }
}