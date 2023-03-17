using Sharara.EntityCodeGen.Generators.Ef;

namespace Sharara.EntityCodeGen
{
    public class Program
    {
        static void Main()
        {
            var def = (new SchemaLoader()).ReadDocument("definition.xml");
            var classGen = new ClassGen(def, entity =>
            {
                Console.WriteLine();
                return Console.Out;
            });
            classGen.Generate();
        }
    }
}
