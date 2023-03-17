using Sharara.EntityCodeGen.Generators;
using Sharara.EntityCodeGen.Generators.Ef;
using Sharara.EntityCodeGen.Generators.Protobuf;

namespace Sharara.EntityCodeGen
{
    public class Program
    {
        static void Main()
        {
            if (!Directory.Exists("Generated"))
            {
                Directory.CreateDirectory("Generated");
            }
            else
            {
                System.IO.DirectoryInfo di = new DirectoryInfo("YourPath");
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }



            var def = (new SchemaLoader()).ReadDocument("definition.xml");
            var classGen = new EntityGen(def, entity =>
            {
                Console.WriteLine();
                return  new CodeWriter(File.CreateText($"Generated/{entity.Name}.cs"));
            });
            classGen.Generate();

            var protoWriter = new CodeWriter(File.CreateText("Generated/output.proto"));
            var protoGen = new ProtoGen(def, protoWriter);
            protoGen.Generate();

            var dbCtxWriter = new CodeWriter(File.CreateText("Generated/DatabaseContext.cs"));
            var dbContextGen = new DbContextGen(def, dbCtxWriter);
            dbContextGen.Generate();
        }

    }
}
