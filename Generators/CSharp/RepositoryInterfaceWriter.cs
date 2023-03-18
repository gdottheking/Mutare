using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    class RepositoryInterfaceWriter : ClassWriter
    {
        protected readonly ICodeGeneratorContext context;

        protected readonly Service service;

        public RepositoryInterfaceWriter(Service service,
            CodeWriter codeWriter,
            ICodeGeneratorContext context)
            : base(codeWriter)
        {
            this.service = service;
            this.context = context;
        }

        protected override IEnumerable<string> Imports => Enumerable.Empty<string>();

        protected override string ClassKeyword => "interface";

        protected override string OutputTypeName => context.GetTypeName(GeneratedType.RepoInterface);

        protected override string Namespace => service.Schema.Configuration.CSharpNamespace;

        protected override string? Implements => null;

        protected override void WriteMethods()
        {
            service.Operations.ToList().ForEach(WriteMethod);
        }

        protected virtual void WriteMethod(Operation op)
        {
            codeWriter.Write(MethodSignature(op))
                .WriteLine(";")
                .WriteLine();
        }

        protected string MethodSignature(Operation op){
            string @params = string.Join(", ", op.Arguments.Select(Format));
            string returnType = CalcReturnType(op.ReturnType);
            return $"{returnType} {op.Name}({@params})";
        }

        protected string CalcReturnType(Object op)
        {
            if (op is FieldType fieldType)
            {
                if (fieldType == FieldType.None)
                {
                    return "void";
                }
                return context.ClrFieldType(fieldType);
            }
            else if (op is Entity ety)
            {
                return context.GetTypeName(ety, GeneratedType.Entity);
            }
            else if (op is Operation.Many many)
            {
                string clrType = CalcReturnType(many.ItemType);
                return $"IEnumerable<{clrType}>";
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected string Format(IArgument arg)
        {
            string typeName;
            if (arg.ArgType is FieldType ftype)
            {
                typeName = context.ClrFieldType(ftype);
            }
            else if (arg.ArgType is Entity entity)
            {
                // TODO: This is incorrect
                typeName = context.GetTypeName(entity, GeneratedType.Entity);
            }
            else
            {
                throw new NotImplementedException();
            }

            return $"{typeName} {arg.Name}";
        }
    }

}