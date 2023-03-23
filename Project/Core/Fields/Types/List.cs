namespace Sharara.EntityCodeGen.Core.Fields.Types
{
    abstract partial class FieldType
    {
        public class List : FieldType
        {
            public List(FieldType itemType) : base(FieldTypeId.List)
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