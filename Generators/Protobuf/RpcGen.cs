using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.Protobuf
{
    internal class RpcServiceGen
    {
        public void GenerateRpcService(Service service, CodeWriter codeWriter)
        {
            WriteServiceDecl(service, codeWriter);
            WriteReqRepMsgs(service, codeWriter);
        }

        void WriteServiceDecl(Service service, CodeWriter codeWriter)
        {
            codeWriter.WriteLine()
                .WriteLines($"service {Common.GrpcServiceName} ", "{")
                .Indent();

            foreach (var operationInfo in service.Operations)
            {
                WriteMethod(codeWriter, operationInfo);
            }

            codeWriter.UnIndent()
                .WriteLine("}")
                .WriteLine();
        }

        void WriteMethod(CodeWriter codeWriter, OperationInfo opInfo)
        {
            string reqMsgName = opInfo.Name + "Request";
            string repMsgName = opInfo.Name + "Response";
            codeWriter.WriteLine($"rpc {opInfo.Name} ({reqMsgName}) returns ({repMsgName});");
        }

        void WriteReqRepMsgs(Service service, CodeWriter codeWriter)
        {
            codeWriter.WriteLines("message Error {").Indent()
                .WriteLines(
                    "int64 ccode = 1;",
                    "string message = 2;"
                ).UnIndent()
                .WriteLine("}")
                .WriteLine();

            codeWriter.WriteLine("message Errors {").Indent()
                .WriteLine("repeated Error errors = 1;").UnIndent()
                .WriteLine("}")
                .WriteLine();

            foreach (var operationInfo in service.Operations)
            {
                WriteReqRepMsgs(codeWriter, operationInfo);
                codeWriter.WriteLine();
            }
        }

        void WriteReqRepMsgs(CodeWriter codeWriter, OperationInfo opInfo)
        {
            string repMsgName = opInfo.Name + "Response";
            codeWriter.WriteLines($"message {repMsgName}", "{")
                .Indent()
                .WriteLine("oneof result {")
                .Indent()
                .WriteLines(
                    "Errors errors = 1;",
                    "int64 payload = 2;"
                ).UnIndent()
                .WriteLine("}")
                .UnIndent()
                .WriteLine("}")
                .WriteLine();


            string reqMsgName = opInfo.Name + "Request";
            codeWriter.WriteLines($"message {reqMsgName}", "{", "}");
        }

    }
}