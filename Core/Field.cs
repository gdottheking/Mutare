namespace Sharara.EntityCodeGen.Core
{
    enum FieldType
    {
        None,
        String,
        Int64,
        Float64,
        DateTime,
        Ref
    }

    class Field
    {
        internal FieldType InternalType { get; }

        public string? Name { get; set; }
        public bool Required { get; set; }
        public bool IsKey { get; set; }
        public bool CheckOnUpdate { get; set; }

        protected Field(FieldType type)
        {
            InternalType = type;
        }

        public override string ToString()
        {
            return $"{Name} [{Required}]";
        }

        public virtual void Accept(IEntityVisitor visitor)
        {
            visitor.VisitField(this);
        }
    }
}