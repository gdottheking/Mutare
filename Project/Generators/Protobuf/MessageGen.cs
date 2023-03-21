using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;
using Sharara.EntityCodeGen.Core.Rpc;
using Sharara.EntityCodeGen.Generators.CSharp;

namespace Sharara.EntityCodeGen.Generators.Protobuf
{
    internal class MessageGen : IEntityVisitor, IFieldVisitor
    {
        private CodeWriter codeWriter;
        private Service service;
        private CodeGeneratorContext context;

        public MessageGen(Service service, CodeWriter codeWriter, CodeGeneratorContext context)
        {
            this.codeWriter = codeWriter;
            this.service = service;
            this.context = context;
        }

        public void Generate()
        {
            var lines = new string[] {
                "syntax = \"proto3\";",
                $"option csharp_namespace = \"{service.Schema.Configuration.CSharpNamespace}.Proto\";",
                $"package {service.Schema.Configuration.ProtoPackage};"
            };

            foreach (var line in lines)
            {
                codeWriter.WriteLine(line);
            }

            codeWriter.WriteLine();

            foreach (var entity in service.Schema.Entities)
            {
                entity.Accept(this);
                codeWriter.WriteLine();
            }

            codeWriter.WriteLines(
                "",
                "//                                 RPC                             //",
                ""
            );

            RpcServiceGen rpcServiceGen = new RpcServiceGen();
            rpcServiceGen.GenerateRpcService(service, codeWriter);

            codeWriter.Flush();
        }

        public void VisitRecord(RecordEntity entity)
        {
            using (codeWriter.CurlyBracketScope($"message {entity.Name} ", false))
            {
                foreach (var field in entity.Fields)
                {
                    field.Accept(this);
                }
            }
            codeWriter.Flush();
        }

        public void VisitEnum(EnumEntity entity)
        {
            using (codeWriter.CurlyBracketScope($"enum {entity.Name} ", false))
            {
                for (int i = 0; i < entity.Values.Count; i++)
                {
                    var val = entity.Values[i];
                    val.Accept(this, i, entity.Values.Count);
                }
            }
            codeWriter.Flush();
        }

        public void VisitField(Field field)
        {
            throw new NotImplementedException();
        }

        private string MapToGrpcType(FieldType fieldType)
        {
            return fieldType switch
            {
                FieldType.DateTime x => x.GrpcType,
                FieldType.Float64 x => x.GrpcType,
                FieldType.Int64 x => x.GrpcType,
                FieldType.Int32 x => x.GrpcType,
                FieldType.String x => x.GrpcType,
                FieldType.Entity entRef => entRef.GetEntity().Name,
                FieldType.List lst => $"repeated ${MapToGrpcType(lst.ItemType)}",
                _ => throw new InvalidOperationException($"{fieldType} does not have a matching protobuf type")
            };
        }

        private void WriteField(string fieldName, int protoId, string protoType)
        {
            codeWriter.WriteLine($"{protoType} {fieldName.ToGrpcNamingConv()} = {protoId};");
        }

        void WriteField(Field field)
        {
            WriteField(field.Name, field.ProtoId, MapToGrpcType(field.FieldType));
        }

        public void VisitStringField(StringField field)
        {
            WriteField(field);
        }

        public void VisitInt64Field(Int64Field field)
        {
            WriteField(field);
        }

        public void VisitInt32Field(Int32Field field)
        {
            WriteField(field);
        }

        public void VisitFloat64Field(Float64Field field)
        {
            WriteField(field);
        }

        public void VisitDateTimeField(DateTimeField field)
        {
            WriteField(field);
        }

        public void VisitReferenceField(ReferenceField field)
        {
            Entity refEntity = context.GetEntity(field.FieldType);
            codeWriter.WriteLine($"{refEntity.Name} {field.Name.ToGrpcNamingConv()} = {field.ProtoId};");
        }

        public void VisitEnumValue(EnumValue value, int offset, int count)
        {
            codeWriter.WriteLine($"{value.Name} = {value.Value};");
        }

        public void VisitListField(ListField listField)
        {
            codeWriter.Write("repeated ");
            var type = (FieldType.List)listField.FieldType;
            WriteField(listField.Name, listField.ProtoId, MapToGrpcType(type.ItemType));
        }
    }
}