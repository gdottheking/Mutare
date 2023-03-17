using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.Ef
{
    internal class EntityGen : IEntityVisitor, IDisposable
    {
        private Func<Entity, CodeWriter> writerProvider;
        private Stack<CodeWriter> writers = new Stack<CodeWriter>();
        private Schema definition;

        static readonly string[] Usings = new string[]{
            "using System.ComponentModel.DataAnnotations;",
            "using System.ComponentModel.DataAnnotations.Schema;"
        };

        public EntityGen(Schema definition, Func<Entity, CodeWriter> writerProvider)
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
            using var codeWriter = writerProvider(entity);

                        foreach (var str in Usings)
            {
                codeWriter.WriteLine(str);
            }
            codeWriter.WriteLine();
            codeWriter.WriteLine($"[Table(\"{entity.Name}\")]");
            codeWriter.WriteLine($"public class {entity.Name}Entity");
            codeWriter.WriteLine("{");
            codeWriter.Indent();
            writers.Push(codeWriter);
            foreach (var field in entity.fields)
            {
                field.Accept(this);
            }
            writers.Pop();
            codeWriter.UnIndent();
            codeWriter.WriteLine("}");
            codeWriter.UnIndent();
            codeWriter.Flush();
        }

        public void VisitEnum(EnumEntity entity)
        {
            using var codeWriter = writerProvider(entity);
            codeWriter.WriteLine($"internal enum {entity.Name}");
            codeWriter.WriteLine("{");
            codeWriter.Indent();
            writers.Push(codeWriter);
            for (int i = 0; i < entity.Values.Count; i++)
            {
                var val = entity.Values[i];
                val.Accept(this, i, entity.Values.Count);
            }
            writers.Pop();
            codeWriter.UnIndent();
            codeWriter.WriteLine("}");
            codeWriter.Flush();
        }

        private void WriteFieldAnnotations(Field field)
        {
            var codeWriter = writers.Peek();

            if (field.IsKey)
            {
                codeWriter.WriteLine($"[Key]");
            }

            if (field.Required)
            {
                codeWriter.WriteLine($"[Required]");
            }

            if (field.CheckOnUpdate)
            {
                codeWriter.WriteLine($"[ConcurrencyCheck]");
            }
        }

        private void WriteField(Field field, string clrType)
        {
            WriteFieldAnnotations(field);
            var codeWriter = writers.Peek();
            codeWriter.WriteLine($"public {clrType} {field.Name} {{get; set;}}");
            codeWriter.WriteLine();
        }

        public void VisitField(Field field)
        {
            throw new NotImplementedException();
        }

        private string ClrFieldType(Field field)
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
            WriteField(field, ClrFieldType(field));
        }

        public void VisitInt64Field(Int64Field field)
        {
            WriteField(field, ClrFieldType(field));
        }

        public void VisitFloat64Field(Float64Field field)
        {
            WriteField(field, ClrFieldType(field));
        }

        public void VisitDateTimeField(DateTimeField field)
        {
            WriteField(field, ClrFieldType(field));
        }

        public void VisitReferenceField(ReferenceField field)
        {
            if (!definition.HasEntityName(field.EntityName))
            {
                throw new InvalidOperationException($"Unknown entity: {field.EntityName}");
            }


            var codeWriter = writers.Peek();
            var refEntity = definition.GetEntityByName(field.EntityName);
            if (refEntity is RecordEntity refRecord)
            {
                foreach (var fkField in refRecord.Keys())
                {
                    WriteFieldAnnotations(field);
                    codeWriter.WriteLine("// TODO: Reference field");
                    codeWriter.WriteLine($"public {ClrFieldType(fkField)} {field.Name}{fkField.Name} {{ get; set; }}");
                }
            }
            else if (refEntity is EnumEntity refEnum)
            {
                WriteFieldAnnotations(field);
                codeWriter.WriteLine("    // TODO: Reference field");
                codeWriter.WriteLine($"public {refEnum.Name} {field.Name} {{ get; set; }}");
            }
        }

        public void VisitEnumValue(EnumValue value, int offset, int count)
        {
            var codeWriter = writers.Peek();
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