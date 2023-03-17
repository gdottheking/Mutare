namespace Sharara.EntityCodeGen.Core
{
    class EnumValue
    {
        public const string ElementName = "value";
        public string Name { get; set; }
        public int Value { get; set; }

        public override string ToString()
        {
            return $"{Name}({Value})";
        }

        public void Accept(IEntityVisitor visitor, int index, int count) {
            visitor.VisitEnumValue(this, index, count);
        }
    }

}