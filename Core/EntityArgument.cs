namespace Sharara.EntityCodeGen.Core
{
    class EntityArgument : Argument<Entity>
    {
        public EntityArgument(Entity Value)
            : base(Value, Value.Name)
        {
        }
    }
}