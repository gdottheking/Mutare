namespace Sharara.EntityCodeGen.Core.Fields
{
    class Field
    {
        internal RecordEntity Record { get; }
        internal FieldType FieldType { get; }

        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public bool IsKey { get; set; }
        public bool CheckOnUpdate { get; set; }
        public int ProtoId { get; set; }

        protected Field(RecordEntity record, FieldType type, string name)
        {
            this.Record = record;
            FieldType = type;
            Name = name;
        }

        public virtual void Accept(IFieldVisitor visitor)
        {
            visitor.VisitField(this);
        }

        public override string ToString()
        {
            return (IsRequired ? "*" : "") + $"{Name}:{FieldType}";
        }
    }
}