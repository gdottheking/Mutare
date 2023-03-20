using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    class ServiceClassWriter : RepositoryInterfaceWriter
    {
        public ServiceClassWriter(Service service,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(service, codeWriter, context)
        {
            Imports.Add("using grpc = global::Grpc.Core;");
            Imports.Add($"using proto = global::{Common.ProtocOutputNamespace};");
        }

        protected override string ClassKeyword => "class";

        protected override string OutputTypeName => context.GetTypeName(GeneratedType.Service);

        protected override string Namespace => service.Schema.Configuration.CSharpNamespace;

        protected override string? Implements => $"proto::{Common.GrpcServiceName}.{Common.GrpcServiceName}Base";

        protected override void WriteConstructor()
        {

            codeWriter.WriteLine($"public {OutputTypeName}(IRepository repo)")
                .WriteLine("{")
                .Indent()
                .WriteLine("this.Repository = repo;")
                .UnIndent()
                .WriteLine("}")
                .WriteLine();

            codeWriter.WriteLine("protected IRepository Repository { get; }")
                .WriteLine();
        }

        protected override void WriteMethod(IProcedure proc)
        {
            var outputNames = new OutputNames(proc);
            switch (proc.ProcedureType)
            {
                case OperationType.Count:
                    WriteOpCount(proc, outputNames);
                    break;

                case OperationType.Put:
                    WriteOpPut(proc, outputNames);
                    break;

                case OperationType.Get:
                    WriteOpGet(proc, outputNames);
                    break;

                case OperationType.List:
                    WriteOpList(proc, outputNames);
                    break;

                case OperationType.Delete:
                    WriteOpDelete(proc, outputNames);
                    break;

                default:
                    OpenMethod(proc, outputNames);
                    codeWriter.WriteLine("throw new NotImplementedException();");
                    CloseMethod();
                    break;
            }
        }

        void OpenMethod(IProcedure proc, OutputNames outputNames)
        {
            string opName = proc.Name;
            string returnType = $"Task<proto::{outputNames.ResponseTypeName}>";
            codeWriter.WriteLines(
                    $"public override async {returnType} {opName}(",
                    $"      proto::{outputNames.RequestTypeName} request,",
                    "      grpc::ServerCallContext context)"
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

        void WriteOpCount(IProcedure proc, OutputNames outputNames)
        {
            OpenMethod(proc, outputNames);

            codeWriter.WriteLines(
                $"var count = await this.Repository.{outputNames.MethodName}();",
                $"var response = new proto::{outputNames.ResponseTypeName} {{ Payload = count }};",
                "return response;"
            );

            CloseMethod();
        }

        void WriteOpGet(IProcedure proc, OutputNames outputNames)
        {
            OpenMethod(proc, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        void WriteOpPut(IProcedure proc, OutputNames outputNames)
        {
            OpenMethod(proc, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        void WriteOpList(IProcedure proc, OutputNames outputNames)
        {
            var args = proc.Arguments;
            var idxArg = args[args.Length - 2];
            var countArg = args[args.Length - 1];
            OpenMethod(proc, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        void WriteOpDelete(IProcedure proc, OutputNames outputNames)
        {
            string entityClassName = context.GetTypeName(proc.Entity, GeneratedType.Entity);
            OpenMethod(proc, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        record OutputNames(string MethodName, string RequestTypeName, string ResponseTypeName)
        {
            public OutputNames(IProcedure proc)
                : this($"{proc.Name}Async",
                    $"{proc.Name}Request",
                    $"{proc.Name}Response"
            )
            {
            }
        }

    }
}