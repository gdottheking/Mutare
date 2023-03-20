using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Rpc;

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

            foreach (var proc in service.Procedures)
            {
                WriteMethod(codeWriter, proc);
            }

            codeWriter.UnIndent()
                .WriteLine("}")
                .WriteLine();
        }

        void WriteMethod(CodeWriter codeWriter, IProcedure proc)
        {
            string reqMsgName = proc.Name + "Request";
            string repMsgName = proc.Name + "Response";
            codeWriter.WriteLine($"rpc {proc.Name} ({reqMsgName}) returns ({repMsgName});");
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

            foreach (var proc in service.Procedures)
            {
                WriteReqRepMsgs(codeWriter, proc);
                codeWriter.WriteLine();
            }
        }

        void WriteReqRepMsgs(CodeWriter codeWriter, IProcedure proc)
        {
            string repMsgName = proc.Name + "Response";
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


            string reqMsgName = proc.Name + "Request";
            codeWriter.WriteLines($"message {reqMsgName}", "{", "}");
        }

    }
}