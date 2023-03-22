using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    sealed class ServiceClassWriter : ClassWriter
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

        CodeGeneratorContext Context { get; }

        Service Service { get; }

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
                    WriteCountProcedure(proc, outputNames);
                    break;

                case OperationType.Put:
                    WritePutProcedure(proc, outputNames);
                    break;

                case OperationType.Get:
                    WriteGetProcedure(proc, outputNames);
                    break;

                case OperationType.List:
                    WriteListProcedure(proc, outputNames);
                    break;

                case OperationType.Delete:
                    WriteDeleteProcedure(proc, outputNames);
                    break;

                default:
                    using (OpenMethod(proc, outputNames))
                    {
                        codeWriter.WriteLine("throw new NotImplementedException();");
                    }
                    break;
            }
        }

        IDisposable OpenMethod(IProcedure proc, ServiceProcClr outputNames)
        {
            string opName = proc.Name;
            string returnType = $"Task<Proto::{outputNames.ResponseTypeName}>";
            codeWriter.WriteLines(
                    $"public override async {returnType} {opName}(",
                    $"      Proto::{outputNames.RequestTypeName} request,"
            );
            return codeWriter.CurlyBracketScope("      Grpc::ServerCallContext context)");
        }

        void WriteCountProcedure(IProcedure proc, ServiceProcClr outputNames)
        {
            using (OpenMethod(proc, outputNames))
            {
                codeWriter.WriteLines(
                    $"var count = await this.Repository.{outputNames.MethodName}();",
                    $"var response = new Proto::{outputNames.ResponseTypeName} {{ Payload = count }};",
                    "return response;"
                );
            }
        }

        void WriteGetProcedure(IProcedure proc, ServiceProcClr outputNames)
        {
            using (OpenMethod(proc, outputNames))
            {
                codeWriter.WriteLine("throw new NotImplementedException();");
            }
        }

        void WritePutProcedure(IProcedure proc, ServiceProcClr outputNames)
        {
            using (OpenMethod(proc, outputNames))
            {
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
            }
        }

        void WriteListProcedure(IProcedure proc, ServiceProcClr outputNames)
        {
            var args = proc.Arguments;
            var idxArg = args[args.Length - 2];
            var countArg = args[args.Length - 1];
            using (OpenMethod(proc, outputNames))
            {
                codeWriter.WriteLine("throw new NotImplementedException();");
            }
        }

        void WriteDeleteProcedure(IProcedure proc, ServiceProcClr outputNames)
        {
            string entityClassName = Context.MapToDotNetType(proc.Record, RecordFile.Entity);
            using (OpenMethod(proc, outputNames))
            {
                codeWriter.WriteLine("throw new NotImplementedException();");
            }
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