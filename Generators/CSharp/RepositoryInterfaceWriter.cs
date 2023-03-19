using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    class RepositoryInterfaceWriter : ClassWriter
    {
        protected readonly CodeGeneratorContext context;

        protected readonly Service service;

        public RepositoryInterfaceWriter(Service service,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(codeWriter)
        {
            this.service = service;
            this.context = context;
        }

        protected override string ClassKeyword => "interface";

        protected override string OutputTypeName => context.GetTypeName(GeneratedType.RepoInterface);

        protected override string Namespace => service.Schema.Configuration.CSharpNamespace;

        protected override string? Implements => null;

        protected override void WriteMethods()
        {
            service.Operations.ToList().ForEach(WriteMethod);
        }

        protected virtual void WriteMethod(OperationInfo op)
        {
            codeWriter.Write(context.ClrDeclString(op))
                .WriteLine(";")
                .WriteLine();
        }
    }

}