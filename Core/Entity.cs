namespace Sharara.EntityCodeGen.Core
{
    abstract class Entity
    {
        public string Name { get; private set; }

        protected Entity(string name) {
            this.Name = name;
        }

        public virtual void Accept(IEntityVisitor visitor) {
            throw new NotImplementedException();
        }

        public  abstract void Validate();


        public override string ToString()
        {
            return $"{this.GetType().Name}({Name})";
        }

    }
}