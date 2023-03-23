namespace Sharara.EntityCodeGen.Core.Fields.Types
{
    abstract partial class FieldType
    {
        public class Float64 : FieldType
        {
            public Float64() : base(FieldTypeId.Float64)
            {
            }
            public static readonly Float64 Instance = new Float64();
            public string ClrType { get; } = "double";
            public override string GrpcType { get; } = "double";
        }
    }
}