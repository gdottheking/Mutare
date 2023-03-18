using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    class RepositoryClassWriter : RepositoryInterfaceWriter
    {
        public RepositoryClassWriter(Service service,
            CodeWriter codeWriter,
            ICodeGeneratorContext context)
            : base(service, codeWriter, context)
        {

        }

        protected override IEnumerable<string> Imports => Enumerable.Empty<string>();

        protected override string ClassKeyword => "class";

        protected override string OutputTypeName => context.GetTypeName(GeneratedType.RepoClass);

        protected override string Namespace => service.Schema.Configuration.CSharpNamespace;

        protected override string? Implements => context.GetTypeName(GeneratedType.RepoInterface);

        protected override void WriteBody()
        {
            codeWriter.WriteLine("private readonly DatabaseContext dbContext;")
                .WriteLine();

            codeWriter.WriteLine($"public {OutputTypeName}(DatabaseContext dbContext)")
                .WriteLine("{")
                .Indent()
                .WriteLine("this.dbContext = dbContext;")
                .UnIndent()
                .WriteLine("}")
                .WriteLine();


            base.WriteBody();
        }
        protected override void WriteMethod(Operation op)
        {
            string sig = MethodSignature(op);
            codeWriter.WriteLine("public " + sig)
                .WriteLine("{")
                .Indent()
                .WriteLine("throw new NotImplementedException();")
                .UnIndent()
                .WriteLine("}")
                .WriteLine();
        }
    }
}