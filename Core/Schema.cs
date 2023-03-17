namespace Sharara.EntityCodeGen.Core
{
    class Schema
    {
        readonly Dictionary<string, Entity> entityById = new Dictionary<string, Entity>();

        internal Schema(params IEnumerable<Entity>[] listOfLists)
        {

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

        public IEnumerable<Entity> Entities { get => entityById.Values; }

        public bool HasEntityName(string name) => entityById.ContainsKey(name);

        public Entity GetEntityByName(string name)
        {
            return entityById[name];
        }

        public void DebugPrint()
        {
            // Display the results
            if (entityById != null)
            {
                foreach (Entity e in entityById.Values)
                {
                    if (e is RecordEntity rec)
                    {
                        Console.WriteLine("record: " + rec.Name);
                        foreach (var field in rec.fields)
                        {
                            Console.WriteLine("    " + field);
                        }
                    }
                    else if (e is EnumEntity en)
                    {
                        Console.WriteLine("enum: " + en.Name);
                        Console.WriteLine("    " + string.Join(",", en.Values.Select(v => v.ToString()))
                        );
                    }
                }
            }
        }

    }

}