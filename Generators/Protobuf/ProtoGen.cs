using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.Protobuf
{
    internal class ProtoGen : IEntityVisitor, IDisposable
    {
        private CodeWriter codeWriter;
        private Schema definition;

        public ProtoGen(Schema definition, CodeWriter codeWriter)
        {
            this.codeWriter = codeWriter;
            this.definition = definition;
        }

        public void Generate()
        {
            foreach (var entity in definition.Entities)
            {
                entity.Accept(this);
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

            codeWriter.WriteLine($"enum {entity.Name}");
            codeWriter.WriteLine("{");
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
            return field.InternalType switch
            {
                FieldType.DateTime => "string",
                FieldType.Float64 => "double",
                FieldType.Int64 => "int64",
                FieldType.String => "string",
                _ => throw new NotImplementedException("FieldType does not have a matching clrType")
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
            if (!definition.HasEntityName(field.EntityName))
            {
                throw new InvalidOperationException($"Unknown entity: {field.EntityName}");
            }

            var refEntity = definition.GetEntityByName(field.EntityName);
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