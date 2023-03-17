namespace Sharara.EntityCodeGen.Core
{
    abstract class Entity
    {
        public string? Name { get; set; }
        public virtual void Accept(IEntityVisitor visitor) {
            throw new NotImplementedException();
        }
    }
}