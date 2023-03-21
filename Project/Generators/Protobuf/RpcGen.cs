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
            using (codeWriter.CurlyBracketScope($"service {Common.GrpcServiceName} ", false))
            {
                foreach (var proc in service.Procedures)
                {
                    WriteMethod(codeWriter, proc);
                }
            }
        }

        void WriteMethod(CodeWriter codeWriter, IProcedure proc)
        {
            string reqMsgName = proc.Name + "Request";
            string repMsgName = proc.Name + "Response";
            codeWriter.WriteLine($"rpc {proc.Name} ({reqMsgName}) returns ({repMsgName});");
        }

        void WriteReqRepMsgs(Service service, CodeWriter codeWriter)
        {
            using (codeWriter.CurlyBracketScope("message Error ", false))
            {
                codeWriter.WriteLines(
                    "int64 ccode = 1;",
                    "string message = 2;"
                );
            }
            codeWriter.WriteLine();

            using (codeWriter.CurlyBracketScope("message Errors ", false))
            {
                codeWriter.WriteLine("repeated Error errors = 1;");
            }
            codeWriter.WriteLine();

            foreach (var proc in service.Procedures)
            {
                WriteReqRepMsgs(codeWriter, proc);
                codeWriter.WriteLine();
            }
        }

        void WriteReqRepMsgs(CodeWriter codeWriter, IProcedure proc)
        {
            string repMsgName = proc.Name + "Response";
            using (codeWriter.CurlyBracketScope($"message {repMsgName} ", false))
            {
                using (codeWriter.CurlyBracketScope("oneof result ", false))
                {
                    codeWriter.WriteLines(
                        "Errors errors = 1;",
                        "int64 payload = 2;"
                    );
                }
            }

            codeWriter.WriteLine();

            string reqMsgName = proc.Name + "Request";
            using (codeWriter.CurlyBracketScope($"message {reqMsgName} ", false))
            {
                codeWriter.WriteLine(
                    $"{proc.Record.Name} {proc.Record.Name.ToGrpcNamingConv()} = 1;"
                );
            }

        }
    }

}
