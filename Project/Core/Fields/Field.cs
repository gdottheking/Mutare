namespace Sharara.EntityCodeGen.Core.Fields
{
    class Field
    {
        internal FieldType FieldType { get; }

        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public bool IsKey { get; set; }
        public bool CheckOnUpdate { get; set; }
        public int ProtoId { get; set; }

        protected Field(FieldType type, string name)
        {
            FieldType = type;
            Name = name;
        }

        public override string ToString()
        {
            return (IsRequired ? "*" : "") + $"{Name}:{FieldType}";
        }

        public virtual void Accept(IFieldVisitor visitor)
        {
            visitor.VisitField(this);
        }
    }
}