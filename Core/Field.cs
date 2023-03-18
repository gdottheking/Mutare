namespace Sharara.EntityCodeGen.Core
{
    class Field
    {
        internal FieldType FieldType { get; }

        public string? Name { get; set; }
        public bool Required { get; set; }
        public bool IsKey { get; set; }
        public bool CheckOnUpdate { get; set; }
        public int ProtoId { get; set; }

        protected Field(FieldType type)
        {
            FieldType = type;
        }

        public override string ToString()
        {
            return $"{Name} [{Required}]";
        }

        public virtual void Accept(IFieldVisitor visitor)
        {
            visitor.VisitField(this);
        }
    }
}