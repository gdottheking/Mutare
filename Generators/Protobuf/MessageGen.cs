using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.Protobuf
{
    internal class MessageGen : IEntityVisitor, IFieldVisitor
    {
        private CodeWriter codeWriter;
        private Service service;
        private RpcServiceGen rpcServiceGen = new RpcServiceGen();

        public MessageGen(Service service, CodeWriter codeWriter)
        {
            this.codeWriter = codeWriter;
            this.service = service;
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

            rpcServiceGen.GenerateRpcService(service, codeWriter);

            foreach (var entity in service.Schema.Entities)
            {
                entity.Accept(this);
                codeWriter.WriteLine();
            }
        }

        public void VisitRecord(RecordEntity entity)
        {
            codeWriter.WriteLine($"message {entity.Name} {{");
            codeWriter.Indent();

            foreach (var field in entity.fields)
            {
                field.Accept(this);
            }

            codeWriter.UnIndent();
            codeWriter.WriteLine("}");
            codeWriter.UnIndent();
            codeWriter.Flush();
        }

        public void VisitEnum(EnumEntity entity)
        {

            codeWriter.WriteLine($"enum {entity.Name} {{");
            codeWriter.Indent();

            for (int i = 0; i < entity.Values.Count; i++)
            {
                var val = entity.Values[i];
                val.Accept(this, i, entity.Values.Count);
            }

            codeWriter.UnIndent();
            codeWriter.WriteLine("}");
            codeWriter.Flush();
        }

        private void WriteField(Field field, string protoType)
        {
            codeWriter.WriteLine($"{protoType} {field.Name} = {field.ProtoId};");
        }

        public void VisitField(Field field)
        {
            throw new NotImplementedException();
        }

        private string ProtoFieldType(Field field)
        {
            return field.FieldType switch
            {
                FieldType.DateTime => "string",
                FieldType.Float64 => "double",
                FieldType.Int64 => "int64",
                FieldType.String => "string",
                _ => throw new NotImplementedException($"{field.FieldType} does not have a matching protobuf type")
            };
        }

        public void VisitStringField(StringField field)
        {
            WriteField(field, ProtoFieldType(field));
        }

        public void VisitInt64Field(Int64Field field)
        {
            WriteField(field, ProtoFieldType(field));
        }

        public void VisitFloat64Field(Float64Field field)
        {
            WriteField(field, ProtoFieldType(field));
        }

        public void VisitDateTimeField(DateTimeField field)
        {
            WriteField(field, ProtoFieldType(field));
        }

        public void VisitReferenceField(ReferenceField field)
        {
            if (!service.Schema.HasEntityName(field.EntityName))
            {
                throw new InvalidOperationException($"Unknown entity: {field.EntityName}");
            }

            var refEntity = service.Schema.GetEntityByName(field.EntityName);
            if (refEntity is RecordEntity refRecord)
            {
                int offset = 0;
                foreach (var fkField in refRecord.Keys())
                {
                    string fieldType = ProtoFieldType(fkField);
                    string fieldName = field.Name + fkField.Name;
                    int protoId = field.ProtoId + offset++;
                    codeWriter.WriteLine($"{fieldType} {fieldName} = {protoId};");
                }
            }
            else if (refEntity is EnumEntity refEnum)
            {
                codeWriter.WriteLine($"{refEnum.Name} {field.Name} = {field.ProtoId};");
            }
        }

        public void VisitEnumValue(EnumValue value, int offset, int count)
        {
            codeWriter.WriteLine($"{value.Name} = {value.Value};");
        }

    }
}