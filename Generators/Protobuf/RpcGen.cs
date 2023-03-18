using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.Protobuf
{
    internal class RpcServiceGen
    {
        public void GenerateRpcService(Schema definition, CodeWriter codeWriter)
        {
            codeWriter.WriteLine()
                .WriteLine("service MyService {")
                .Indent()
                .WriteLine();

            foreach (var entity in definition.Entities)
            {
                if (entity is RecordEntity rec)
                {
                    WriteCrudMethods(codeWriter, rec);
                    codeWriter.WriteLine();
                }
            }

            codeWriter.UnIndent()
                .WriteLine("}")
                .WriteLine();
        }

        void WriteCrudMethods(CodeWriter codeWriter, RecordEntity rec)
        {
            codeWriter.WriteLine($"rpc Create{rec.Name} (Create{rec.Name}Request) returns (Create{rec.Name}Response);")
                .WriteLine($"rpc Get{rec.Name} (Get{rec.Name}Request) returns (Get{rec.Name}Response);")
                .WriteLine($"rpc Update{rec.Name} (Update{rec.Name}Request) returns (Update{rec.Name}Response);")
                .WriteLine($"rpc Delete{rec.Name} (Delete{rec.Name}Request) returns (Delete{rec.Name}Response);")
                .WriteLine($"rpc List{rec.Name} (List{rec.Name}Request) returns (List{rec.Name}Response);")
                .WriteLine($"rpc Get{rec.Name}Count (Get{rec.Name}CountRequest) returns (Get{rec.Name}CountResponse);");
        }
    }
}