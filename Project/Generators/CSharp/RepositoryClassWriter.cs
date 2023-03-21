using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Rpc;

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

        protected override void WriteMethod(IProcedure proc)
        {
            switch (proc.ProcedureType)
            {
                case OperationType.Count:
                    WriteOpCount(proc);
                    break;

                case OperationType.Put:
                    WriteOpPut(proc);
                    break;

                case OperationType.Get:
                    WriteOpGet(proc);
                    break;

                case OperationType.List:
                    WriteOpList(proc);
                    break;

                case OperationType.Delete:
                    WriteOpDelete(proc);
                    break;

                default:
                    OpenMethod(proc);
                    codeWriter.WriteLine("throw new NotImplementedException();");
                    CloseMethod();
                    break;
            }
        }

        void OpenMethod(IProcedure proc)
        {
            string opSignature = context.ClrDeclString(proc);
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

        void WriteOpCount(IProcedure proc)
        {
            OpenMethod(proc);

            string opReturnType = context.MapToDotNetType(proc.ReturnType);

            codeWriter.WriteLines(
                $"var num = dbContext.{proc.Record.Name}.LongCount();",
                $"return await(new ValueTask<long>(num));"
            );

            CloseMethod();
        }

        void WriteOpGet(IProcedure proc)
        {
            OpenMethod(proc);

            var args = string.Join(", ", proc.Arguments.Select(x => x.Name));
            codeWriter.WriteLine($"return await dbContext.{proc.Record.Name}.FindAsync({args});");

            CloseMethod();
        }

        void WriteOpPut(IProcedure proc)
        {
            OpenMethod(proc);

            codeWriter.WriteLines(
                $"await dbContext.{proc.Record.Name}.AddAsync({proc.Arguments[0].Name});",
                "await dbContext.SaveChangesAsync();"
            );

            CloseMethod();
        }

        void WriteOpList(IProcedure proc)
        {
            var args = proc.Arguments;
            var idxArg = args[args.Length-2];
            var countArg = args[args.Length-1];
            OpenMethod(proc);

            var retType = context.MapToDotNetType(proc.ReturnType);

            codeWriter.WriteLines(
                $"{idxArg.Name} = Math.Max(0, {idxArg.Name});",
                $"var offset = (int)({idxArg.Name} * {countArg.Name});",
                $"var items = dbContext.{proc.Record.Name}",
                $"   .Skip(offset)",
                $"   .Take((int) {countArg.Name})",
                "   .AsNoTracking();",
                $"return await (new ValueTask<{retType}>(items));"
            );

            CloseMethod();
        }

        void WriteOpDelete(IProcedure proc)
        {
            string entityClassName = context.GetTypeName(proc.Record, GeneratedType.Entity);
            OpenMethod(proc);

            var assigns = string.Join(", ",
                proc.Arguments.Select(x => $"{proc.Record[x.Name!]?.Name} = {x.Name}")
            );

            codeWriter.WriteLines(
                $"var ety = new {entityClassName} {{ {assigns} }};",
                $"dbContext.{proc.Record.Name}.Remove(ety);",
                "await dbContext.SaveChangesAsync();"
            );

            CloseMethod();
        }
    }
}