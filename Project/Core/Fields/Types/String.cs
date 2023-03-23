namespace Sharara.EntityCodeGen.Core.Fields.Types
{
    abstract partial class FieldType
    {
        public class String : FieldType
        {
            public String() : base(FieldTypeId.String)
            {
            }
            public static readonly String Instance = new String();
            public string ClrType { get; } = "string";
            public override string GrpcType { get; } = "string";
        }
    }
}