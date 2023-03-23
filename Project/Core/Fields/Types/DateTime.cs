namespace Sharara.EntityCodeGen.Core.Fields.Types
{
    abstract partial class FieldType
    {
        public class DateTime : FieldType
        {
            public DateTime() : base(FieldTypeId.DateTime)
            {
            }

            public static readonly DateTime Instance = new DateTime();
            public string ClrType { get; } = "DateTime";
            public override string GrpcType { get; } = "string";
        }
    }
}