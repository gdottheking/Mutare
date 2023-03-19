using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    class RepositoryClassWriter : RepositoryInterfaceWriter
    {
        public RepositoryClassWriter(Service service,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(service, codeWriter, context)
        {
            Imports.Add("using Microsoft.EntityFrameworkCore;");
        }

        protected override string ClassKeyword => "class";

        protected override string OutputTypeName => context.GetTypeName(GeneratedType.RepoClass);

        protected override string Namespace => service.Schema.Configuration.CSharpNamespace;

        protected override string? Implements => context.GetTypeName(GeneratedType.RepoInterface);

        protected override void WriteBody()
        {
            codeWriter
                .WriteLine("private readonly DatabaseContext dbContext;")
                .WriteLine();

            base.WriteBody();
        }

        protected override void WriteConstructor()
        {
            codeWriter.WriteLine($"public {OutputTypeName}(DatabaseContext dbContext)")
                .WriteLine("{")
                .Indent()
                .WriteLine("this.dbContext = dbContext;")
                .UnIndent()
                .WriteLine("}")
                .WriteLine();
        }

        protected override void WriteMethod(OperationInfo opInfo)
        {
            switch (opInfo.OperationType)
            {
                case OperationType.Count:
                    WriteOpCount(opInfo);
                    break;

                case OperationType.Put:
                    WriteOpPut(opInfo);
                    break;

                case OperationType.Get:
                    WriteOpGet(opInfo);
                    break;

                case OperationType.List:
                    WriteOpList(opInfo);
                    break;

                case OperationType.Delete:
                    WriteOpDelete(opInfo);
                    break;

                default:
                    OpenMethod(opInfo);
                    codeWriter.WriteLine("throw new NotImplementedException();");
                    CloseMethod();
                    break;
            }
        }

        void OpenMethod(OperationInfo operationInfo)
        {
            string opSignature = context.ClrDeclString(operationInfo);
            codeWriter.WriteLine("public async " + opSignature)
                .WriteLine("{")
                .Indent();
        }

        void CloseMethod()
        {
            codeWriter.UnIndent()
                .WriteLine("}")
                .WriteLine();
        }

        void WriteOpCount(OperationInfo operationInfo)
        {
            OpenMethod(operationInfo);

            string opReturnType = context.MapToClrTypeName(operationInfo.ReturnType);

            codeWriter.WriteLines(
                $"var num = dbContext.{operationInfo.Entity.Name}.LongCount();",
                $"return await(new ValueTask<long>(num));"
            );

            CloseMethod();
        }

        void WriteOpGet(OperationInfo operationInfo)
        {
            OpenMethod(operationInfo);

            var args = string.Join(", ", operationInfo.Arguments.Select(x => x.Name));
            codeWriter.WriteLine($"return await dbContext.{operationInfo.Entity.Name}.FindAsync({args});");

            CloseMethod();
        }

        void WriteOpPut(OperationInfo operationInfo)
        {
            OpenMethod(operationInfo);

            codeWriter.WriteLines(
                $"await dbContext.{operationInfo.Entity.Name}.AddAsync({operationInfo.Arguments[0].Name});",
                "await dbContext.SaveChangesAsync();"
            );

            CloseMethod();
        }

        void WriteOpList(OperationInfo operationInfo)
        {
            var args = operationInfo.Arguments;
            var idxArg = args[args.Length-2];
            var countArg = args[args.Length-1];
            OpenMethod(operationInfo);

            var retType = context.MapToClrTypeName(operationInfo.ReturnType);

            codeWriter.WriteLines(
                $"{idxArg.Name} = Math.Max(0, {idxArg.Name});",
                $"var offset = (int)({idxArg.Name} * {countArg.Name});",
                $"var items = dbContext.{operationInfo.Entity.Name}",
                $"   .Skip(offset)",
                $"   .Take((int) {countArg.Name})",
                "   .AsNoTracking();",
                $"return await (new ValueTask<{retType}>(items));"
            );

            CloseMethod();
        }

        void WriteOpDelete(OperationInfo operationInfo)
        {
            string entityClassName = context.GetTypeName(operationInfo.Entity, GeneratedType.Entity);
            OpenMethod(operationInfo);

            var assigns = string.Join(", ", operationInfo.Arguments.Select(x => $"{x.Name} = {x.Name}"));

            codeWriter.WriteLines(
                $"var ety = new {entityClassName} {{ {assigns} }};",
                $"dbContext.{operationInfo.Entity.Name}.Remove(ety);",
                "await dbContext.SaveChangesAsync();"
            );

            CloseMethod();
        }
    }
}