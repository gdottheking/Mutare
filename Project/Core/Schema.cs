namespace Sharara.EntityCodeGen.Core
{
    class Schema
    {
        private readonly Dictionary<string, Entity> entityById = new Dictionary<string, Entity>();

        public Schema(SchemaConfig config, params IEnumerable<Entity>[] listOfLists)
        {
            this.Configuration = config;
            Action<IEnumerable<Entity>> AddItems = (items) =>
            {
                foreach (var item in items)
                {
                    if (entityById.ContainsKey(item.Name!))
                    {
                        throw new InvalidOperationException($"An entity with the name {item.Name} already exists");
                    }
                    entityById.Add(item.Name!, item);
                }
            };

            foreach (var list in listOfLists)
            {
                AddItems(list);
            }

        }

        public SchemaConfig Configuration { get; }

        public IEnumerable<Entity> Entities { get => entityById.Values; }

        public bool HasEntityName(string name) => entityById.ContainsKey(name);

        public Entity GetEntityByName(string name)
        {
            return entityById[name];
        }

        public void Validate()
        {
            foreach (var entity in Entities)
            {
                entity.Validate();
            }
        }

    }

}