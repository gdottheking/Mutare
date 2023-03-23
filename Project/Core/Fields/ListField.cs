using Sharara.EntityCodeGen.Core.Fields.Types;

namespace Sharara.EntityCodeGen.Core.Fields
{
    record class ListField : Field
    {
        public const string XmlTypeName = "list";

        public ListField(RecordEntity record, FieldType itemType, string name)
            : base(record, listType(itemType), name)
        {
        }

        internal new FieldType.List FieldType => (FieldType.List)base.FieldType;

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.VisitListField(this);
        }
    }
}