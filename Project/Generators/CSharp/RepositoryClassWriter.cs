using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    sealed class RepositoryClassWriter : ClassWriter
    {
        public RepositoryClassWriter(Service service,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(codeWriter)
        {
            Imports.Add("using Microsoft.EntityFrameworkCore;");
            this.Service = service;
            this.Context = context;
        }

        CodeGeneratorContext Context { get; }

        Service Service { get; }

        protected override string TargetTypeName => Context.RepositoryClassName;

        protected override CSharpTargetType TargetType => CSharpTargetType.Class;

        protected override string Namespace => Service.Schema.Configuration.CSharpNamespace;

        protected override string? Implements => Context.RepositoryInterfaceName;

        protected override void WriteBody()
        {
            codeWriter
                .WriteLine("private readonly DatabaseContext dbContext;")
                .WriteLine();

            base.WriteBody();
        }

        protected override void WriteConstructor()
        {
            codeWriter.WriteLine($"public {TargetTypeName}(DatabaseContext dbContext)")
                .WriteLine("{")
                .Indent()
                .WriteLine("this.dbContext = dbContext;")
                .UnIndent()
                .WriteLine("}")
                .WriteLine();
        }

        protected override void WriteMethods()
        {
            Service.Procedures.ToList().ForEach(WriteMethod);
        }

        private void WriteMethod(IProcedure proc)
        {
            switch (proc.ProcedureType)
            {
                case ProcedureType.Count:
                    WriteCountProcedure(proc);
                    break;

                case ProcedureType.Put:
                    WritePutProcedure(proc);
                    break;

                case ProcedureType.Get:
                    WriteGetProcedure(proc);
                    break;

                case ProcedureType.List:
                    WriteListProcedure(proc);
                    break;

                case ProcedureType.Delete:
                    WriteDeleteProcedure(proc);
                    break;

                default:
                    using (OpenMethod(proc))
                    {
                        codeWriter.WriteLine("throw new NotImplementedException();");
                    }
                    break;
            }
        }

        IDisposable OpenMethod(IProcedure proc)
        {
            string opSignature = Context.ClrDeclString(proc);
            return codeWriter.CurlyBracketScope("public async " + opSignature);
        }

        void WriteCountProcedure(IProcedure proc)
        {
            using (OpenMethod(proc))
            {
                string opReturnType = Context.MapToDotNetType(proc.ReturnType);
                codeWriter.WriteLines(
                    $"var num = dbContext.{proc.Record.Name}.LongCount();",
                    $"return await(new ValueTask<long>(num));"
                );
            }
        }

        void WriteGetProcedure(IProcedure proc)
        {
            using (OpenMethod(proc))
            {
                var args = string.Join(", ", proc.Arguments.Select(x => x.Name));
                codeWriter.WriteLine($"return await dbContext.{proc.Record.Name}.FindAsync({args});");
            }
        }

        void WritePutProcedure(IProcedure proc)
        {
            using (OpenMethod(proc))
            {
                codeWriter.WriteLines(
                    $"await dbContext.{proc.Record.Name}.AddAsync({proc.Arguments[0].Name});",
                    "await dbContext.SaveChangesAsync();"
                );
            }
        }

        void WriteListProcedure(IProcedure proc)
        {
            var args = proc.Arguments;
            var idxArg = args[args.Length - 2];
            var countArg = args[args.Length - 1];

            using (OpenMethod(proc))
            {

                var retType = Context.MapToDotNetType(proc.ReturnType);

                codeWriter.WriteLines(
                    $"{idxArg.Name} = Math.Max(0, {idxArg.Name});",
                    $"var offset = (int)({idxArg.Name} * {countArg.Name});",
                    $"var items = dbContext.{proc.Record.Name}",
                    $"   .Skip(offset)",
                    $"   .Take((int) {countArg.Name})",
                    "   .AsNoTracking();",
                    $"return await (new ValueTask<{retType}>(items));"
                );

            }
        }

        void WriteDeleteProcedure(IProcedure proc)
        {
            string entityClassName = Context.MapToDotNetType(proc.Record, RecordFile.Entity);
            using (OpenMethod(proc))
            {
                var assigns = string.Join(", ",
                    proc.Arguments.Select(x => $"{proc.Record[x.Name!]?.Name} = {x.Name}")
                );

                codeWriter.WriteLines(
                    $"var ety = new {entityClassName} {{ {assigns} }};",
                    $"dbContext.{proc.Record.Name}.Remove(ety);",
                    "await dbContext.SaveChangesAsync();"
                );
            }
        }
    }
}