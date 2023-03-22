using System.Text.RegularExpressions;
using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Rpc;
using Sharara.EntityCodeGen.Generators;
using Sharara.EntityCodeGen.Generators.CSharp;
using Sharara.EntityCodeGen.Generators.Protobuf;

namespace Sharara.EntityCodeGen
{
    public partial class Program
    {
        private const string OutputFolder = "../Example/Generated";

        static void Main()
        {
            Console.WriteLine("Loading schema");
            var schema = (new SchemaLoader()).ReadDocument("example.xml");
            var service = new Service(schema);

            CleanOutputFolder();

            var context = new CodeGeneratorContext(OutputFolder);

            Console.WriteLine("Generating Entity classes");
            var generator = new Generator(service, context);
            generator.Generate();

            Console.WriteLine("Generating DatabaseContext");
            var dbCtxWriter = new CodeWriter(File.CreateText($"{OutputFolder}/DatabaseContext.cs"));
            var dbContextGen = new DatabaseContextWriter(schema, dbCtxWriter, context);
            dbContextGen.Generate();

            Console.WriteLine("Generating proto file");
            var protoWriter = new CodeWriter(File.CreateText($"{OutputFolder}/AutoGenService.proto"));
            var protoGen = new MessageGen(service, protoWriter, context);
            protoGen.Generate();
        }

        static void CleanOutputFolder()
        {
            Console.WriteLine($"Cleaning folder {OutputFolder}");

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }
            else
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(OutputFolder);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
        }

    }

}
