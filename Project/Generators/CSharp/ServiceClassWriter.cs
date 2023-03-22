using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    class ServiceClassWriter : ClassWriter
    {

        public ServiceClassWriter(Service service,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(codeWriter)
        {
            Imports.Add("using Grpc = global::Grpc.Core;");
            Imports.Add($"using Proto = global::{Common.ProtocOutputNamespace};");
            Imports.Add("using System.ComponentModel.DataAnnotations;");
            this.Context = context;
            this.Service = service;
        }

        protected CodeGeneratorContext Context { get; }

        protected Service Service { get; }

        protected override string TargetTypeName => Context.ServiceClassName;

        protected override string Namespace => Service.Schema.Configuration.CSharpNamespace;

        protected override string? Implements => $"Proto::{Common.GrpcServiceName}.{Common.GrpcServiceName}Base";

        protected override void WriteConstructor()
        {
            codeWriter.WriteLine($"public {TargetTypeName}(IRepository repo)")
                .WriteLine("{")
                .Indent()
                .WriteLine("this.Repository = repo;")
                .UnIndent()
                .WriteLine("}")
                .WriteLine();

            codeWriter.WriteLine("protected IRepository Repository { get; }")
                .WriteLine();
        }

        protected override void WriteMethods()
        {
             Service.Procedures.ToList().ForEach(WriteMethod);
        }

        private void WriteMethod(IProcedure proc)
        {
            var outputNames = new ServiceProcClr(proc);
            switch (proc.ProcedureType)
            {
                case OperationType.Count:
                    WriteProcCount(proc, outputNames);
                    break;

                case OperationType.Put:
                    WriteProcPut(proc, outputNames);
                    break;

                case OperationType.Get:
                    WriteProcGet(proc, outputNames);
                    break;

                case OperationType.List:
                    WriteProcList(proc, outputNames);
                    break;

                case OperationType.Delete:
                    WriteProcDelete(proc, outputNames);
                    break;

                default:
                    OpenMethod(proc, outputNames);
                    codeWriter.WriteLine("throw new NotImplementedException();");
                    CloseMethod();
                    break;
            }
        }

        void OpenMethod(IProcedure proc, ServiceProcClr outputNames)
        {
            string opName = proc.Name;
            string returnType = $"Task<Proto::{outputNames.ResponseTypeName}>";
            codeWriter.WriteLines(
                    $"public override async {returnType} {opName}(",
                    $"      Proto::{outputNames.RequestTypeName} request,",
                    "      Grpc::ServerCallContext context)"
                )
                .WriteLine("{")
                .Indent();
        }

        void CloseMethod()
        {
            codeWriter.UnIndent()
                .WriteLine("}")
                .WriteLine();
        }

        void WriteProcCount(IProcedure proc, ServiceProcClr outputNames)
        {
            OpenMethod(proc, outputNames);

            codeWriter.WriteLines(
                $"var count = await this.Repository.{outputNames.MethodName}();",
                $"var response = new Proto::{outputNames.ResponseTypeName} {{ Payload = count }};",
                "return response;"
            );

            CloseMethod();
        }

        void WriteProcGet(IProcedure proc, ServiceProcClr outputNames)
        {
            OpenMethod(proc, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        void WriteProcPut(IProcedure proc, ServiceProcClr outputNames)
        {
            OpenMethod(proc, outputNames);
            var converterClassName = Context.MapToDotNetType(proc.Record, RecordFile.Converter);
            var entClassName = Context.MapToDotNetType(proc.Record, RecordFile.Entity);
            codeWriter.WriteLine($"var input = new {entClassName}();");
            codeWriter.WriteLines(
                    "",
                    "ValidationContext validationCtx = new (input);",
                    "input.Validate(validationCtx);",
                    $"await this.Repository.{outputNames.MethodName}(input);",
                    $"Proto::{outputNames.ResponseTypeName} response = new();",
                    $"return response;"
                );

            CloseMethod();
        }

        void WriteProcList(IProcedure proc, ServiceProcClr outputNames)
        {
            var args = proc.Arguments;
            var idxArg = args[args.Length - 2];
            var countArg = args[args.Length - 1];
            OpenMethod(proc, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        void WriteProcDelete(IProcedure proc, ServiceProcClr outputNames)
        {
            string entityClassName = Context.MapToDotNetType(proc.Record, RecordFile.Entity);
            OpenMethod(proc, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        protected override void WriteFields()
        {
            base.WriteFields();
            codeWriter.WriteLine($"const string DateFormatString = \"{Common.DateTimeFormatString}\";");

            HashSet<string> converterClassNames = new HashSet<string>();
            foreach (var entity in Service.Schema.Entities)
            {
                if (entity.EntityType == Core.EntityType.Enum)
                {
                    converterClassNames.Add(Context.MapToDotNetType((Core.EnumEntity)entity, EnumFile.Converter));
                }
                else
                {
                    converterClassNames.Add(Context.MapToDotNetType((Core.RecordEntity)entity, RecordFile.Converter));
                }
            }

            foreach (string className in converterClassNames)
            {
                codeWriter.WriteLine($"private static readonly {className} {className.ToCamelCase()} = new();");
            }

            codeWriter.WriteLine();
        }
    }
}