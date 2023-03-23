namespace Sharara.EntityCodeGen.Core.Fields.Types
{
    abstract partial class FieldType
    {
        public abstract string GrpcType { get; }

        public FieldTypeId Id { get; }

        protected FieldType(FieldTypeId id)
        {
            this.Id = id;
        }
    }
}