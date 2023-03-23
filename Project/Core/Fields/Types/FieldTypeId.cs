namespace Sharara.EntityCodeGen.Core.Fields.Types
{
    abstract partial class FieldType
    {
        public enum FieldTypeId
        {
            Void,
            String,
            Int32,
            Int64,
            Float64,
            DateTime,
            Entity,
            List
        }
    }
}