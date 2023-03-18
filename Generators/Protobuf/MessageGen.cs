using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.Protobuf
{
    internal class MessageGen : IEntityVisitor, IFieldVisitor, IDisposable
    {
        private CodeWriter codeWriter;
        private Schema schema;
        private RpcServiceGen rpcServiceGen = new RpcServiceGen();

        public MessageGen(Schema definition, CodeWriter codeWriter)
        {
            this.codeWriter = codeWriter;
            this.schema = definition;
        }

        public void Generate()
        {
            var lines = new string[] {
                "syntax = \"proto3\"",
                $"option csharp_namespace = \"{schema.Configuration.CSharpNamespace}\";",
                $"package {schema.Configuration.ProtoPackage};"
            };

            foreach (var line in lines)
            {
                codeWriter.WriteLine(line);
            }

            codeWriter.WriteLine();

            rpcServiceGen.GenerateRpcService(schema, codeWriter);

            foreach (var entity in schema.Entities)
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
            if (!schema.HasEntityName(field.EntityName))
            {
                throw new InvalidOperationException($"Unknown entity: {field.EntityName}");
            }

            var refEntity = schema.GetEntityByName(field.EntityName);
            if (refEntity is RecordEntity refRecord)
            {
                foreach (var fkField in refRecord.Keys())
                {
                    codeWriter.WriteLine("// TODO: Reference field");
                    codeWriter.WriteLine($"public {ProtoFieldType(fkField)} {field.Name}{fkField.Name} {{ get; set; }}");
                }
            }
            else if (refEntity is EnumEntity refEnum)
            {
                codeWriter.WriteLine("// TODO: Reference field");
                codeWriter.WriteLine($"public {refEnum.Name} {field.Name} {{ get; set; }}");
            }
        }

        public void VisitEnumValue(EnumValue value, int offset, int count)
        {
            codeWriter.Write($"{value.Name} = {value.Value}");
            if (offset < count - 1)
            {
                codeWriter.Write(",");
            }
            codeWriter.WriteLine();
        }

        public void Dispose()
        {
        }
    }
}