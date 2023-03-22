using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    sealed class RepositoryInterfaceWriter : ClassWriter
    {
        private readonly CodeGeneratorContext context;

        private readonly Service service;

        public RepositoryInterfaceWriter(Service service,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(codeWriter)
        {
            this.service = service;
            this.context = context;
        }

        protected override CSharpTargetType TargetType => CSharpTargetType.Interface;

        protected override string TargetTypeName => context.RepositoryInterfaceName;

        protected override string Namespace => service.Schema.Configuration.CSharpNamespace;

        protected override string? Implements => null;

        protected override void WriteMethods()
        {
            service.Procedures.ToList().ForEach(WriteMethod);
        }

        private void WriteMethod(IProcedure proc)
        {
            codeWriter.Write(context.ClrDeclString(proc))
                .WriteLine(";")
                .WriteLine();
        }
    }

}