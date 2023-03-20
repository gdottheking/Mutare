using Sharara.EntityCodeGen.Core;
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

        protected override void WriteMethod(OperationInfo operationInfo)
        {
            var outputNames = new OutputNames(operationInfo);
            switch (operationInfo.OperationType)
            {
                case OperationType.Count:
                    WriteOpCount(operationInfo, outputNames);
                    break;

                case OperationType.Put:
                    WriteOpPut(operationInfo, outputNames);
                    break;

                case OperationType.Get:
                    WriteOpGet(operationInfo, outputNames);
                    break;

                case OperationType.List:
                    WriteOpList(operationInfo, outputNames);
                    break;

                case OperationType.Delete:
                    WriteOpDelete(operationInfo, outputNames);
                    break;

                default:
                    OpenMethod(operationInfo, outputNames);
                    codeWriter.WriteLine("throw new NotImplementedException();");
                    CloseMethod();
                    break;
            }
        }

        void OpenMethod(OperationInfo operationInfo, OutputNames outputNames)
        {
            string opName = operationInfo.Name;
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

        void WriteOpCount(OperationInfo operationInfo, OutputNames outputNames)
        {
            OpenMethod(operationInfo, outputNames);

            codeWriter.WriteLines(
                $"var count = await this.Repository.{outputNames.MethodName}();",
                $"var response = new proto::{outputNames.ResponseTypeName} {{ Payload = count }};",
                "return response;"
            );

            CloseMethod();
        }

        void WriteOpGet(OperationInfo operationInfo, OutputNames outputNames)
        {
            OpenMethod(operationInfo, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        void WriteOpPut(OperationInfo operationInfo, OutputNames outputNames)
        {
            OpenMethod(operationInfo, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        void WriteOpList(OperationInfo operationInfo, OutputNames outputNames)
        {
            var args = operationInfo.Arguments;
            var idxArg = args[args.Length - 2];
            var countArg = args[args.Length - 1];
            OpenMethod(operationInfo, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        void WriteOpDelete(OperationInfo operationInfo, OutputNames outputNames)
        {
            string entityClassName = context.GetTypeName(operationInfo.Entity, GeneratedType.Entity);
            OpenMethod(operationInfo, outputNames);

            codeWriter.WriteLine("throw new NotImplementedException();");

            CloseMethod();
        }

        record OutputNames(string MethodName, string RequestTypeName, string ResponseTypeName)
        {
            public OutputNames(OperationInfo operationInfo)
                : this($"{operationInfo.Name}Async",
                    $"{operationInfo.Name}Request",
                    $"{operationInfo.Name}Response"
            )
            {
            }
        }

    }
}