using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.Ef
{
    internal class ClassGen : IEntityVisitor, IDisposable
    {
        const string Padding1 = "    ";
        const string Padding2 = Padding1 + Padding1;

        private Func<Entity, TextWriter> writerProvider;
        private Stack<TextWriter> writers = new Stack<TextWriter>();
        private Schema definition;

        public ClassGen(Schema definition, Func<Entity, TextWriter> writerProvider)
        {
            this.writerProvider = writerProvider;
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
            using var writer = writerProvider(entity);
            writer.WriteLine($"internal class {entity.Name}");
            writer.WriteLine("{");
            writer.Flush();
            writers.Push(writer);
            foreach (var field in entity.fields)
            {
                field.Accept(this);
            }
            writers.Pop();
            writer.WriteLine("}");
            writer.Flush();
        }

        public void VisitEnum(EnumEntity entity)
        {
            using var writer = writerProvider(entity);
            writer.WriteLine($"internal enum {entity.Name}");
            writer.WriteLine("{");
            writers.Push(writer);
            for (int i = 0; i < entity.Values.Count; i++)
            {
                var val = entity.Values[i];
                val.Accept(this, i, entity.Values.Count);
            }
            writers.Pop();
            writer.WriteLine("}");
            writer.Flush();
        }

        private void WriteFieldAnnotations(Field field)
        {
            var writer = writers.Peek();

            if (field.IsKey)
            {
                writer.WriteLine($"{Padding1}[Key]");
            }

            if (field.Required)
            {
                writer.WriteLine($"{Padding1}[Required]");
            }

            if (field.CheckOnUpdate)
            {
                writer.WriteLine($"{Padding1}[ConcurrencyCheck]");
            }
        }
        private void WriteField(Field field, string clrType)
        {
            WriteFieldAnnotations(field);
            var writer = writers.Peek();
            writer.WriteLine($"{Padding1}public {clrType} {field.Name} {{get; set;}}");
            writer.WriteLine();
        }

        public void VisitField(Field field)
        {
            throw new NotImplementedException();
        }

        private string clrType(Field field)
        {
            return field.InternalType switch
            {
                FieldType.DateTime => "DateTime",
                FieldType.Float64 => "double",
                FieldType.Int64 => "long",
                FieldType.String => "string",
                _ => throw new NotImplementedException("FieldType does not have a matching clrType")
            };
        }

        public void VisitStringField(StringField field)
        {
            WriteField(field, clrType(field));
        }

        public void VisitInt64Field(Int64Field field)
        {
            WriteField(field, clrType(field));
        }

        public void VisitFloat64Field(Float64Field field)
        {
            WriteField(field, clrType(field));
        }

        public void VisitDateTimeField(DateTimeField field)
        {
            WriteField(field, clrType(field));
        }

        public void VisitReferenceField(ReferenceField field)
        {
            if (!definition.HasEntityName(field.EntityName))
            {
                throw new InvalidOperationException($"Unknown entity: {field.EntityName}");
            }


            var writer = writers.Peek();
            var refEntity = definition.GetEntityByName(field.EntityName);
            if (refEntity is RecordEntity refRecord)
            {
                foreach (var fkField in refRecord.Keys())
                {
                    WriteFieldAnnotations(field);
                    writer.WriteLine("    // TODO: Reference field");
                    writer.WriteLine($"{Padding1}public {clrType(fkField)} {field.Name}{fkField.Name} {{ get; set; }}");
                }
            }
            else if (refEntity is EnumEntity refEnum)
            {
                WriteFieldAnnotations(field);
                writer.WriteLine("    // TODO: Reference field");
                writer.WriteLine($"{Padding1}public {refEnum.Name} {field.Name} {{ get; set; }}");
            }
        }

        public void VisitEnumValue(EnumValue value, int offset, int count)
        {
            var writer = writers.Peek();
            writer.Write($"    {value.Name} = {value.Value}");
            if (offset < count - 1)
            {
                writer.Write(",");
            }
            writer.WriteLine();
        }

        public void Dispose()
        {
        }
    }
}