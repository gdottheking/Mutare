using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal class EntityClassWriter : ClassWriter, IFieldVisitor
    {
        private Schema schema;
        private ICodeGeneratorContext context;
        private readonly RecordEntity record;
        static readonly string[] imports = new string[]{
            "using System.ComponentModel.DataAnnotations;",
            "using System.ComponentModel.DataAnnotations.Schema;"
        };

        public EntityClassWriter(RecordEntity record, Schema definition, CodeWriter codeWriter, ICodeGeneratorContext context)
            : base(codeWriter)
        {
            this.record = record;
            this.schema = definition;
            this.context = context;
        }

        protected override IEnumerable<string> Imports => imports;

        protected override string OutputTypeName => context.GetTypeName(record, GeneratedType.Entity);

        protected override string Namespace => schema.Configuration.CSharpNamespace;

        protected override void WriteFields()
        {

            foreach (var field in record.fields)
            {
                field.Accept(this);
            }

        }

        public void VisitField(Field field)
        {
            throw new NotImplementedException();
        }

        private void WriteFieldAnnotations(Field field)
        {
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
            codeWriter.WriteLine($"public {clrType} {field.Name} {{get; set;}}");
            codeWriter.WriteLine();
        }

        public void VisitStringField(StringField field)
        {
            WriteField(field, context.ClrFieldType(field.FieldType));
        }

        public void VisitInt64Field(Int64Field field)
        {
            WriteField(field, context.ClrFieldType(field.FieldType));
        }

        public void VisitFloat64Field(Float64Field field)
        {
            WriteField(field, context.ClrFieldType(field.FieldType));
        }

        public void VisitDateTimeField(DateTimeField field)
        {
            WriteField(field, context.ClrFieldType(field.FieldType));
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
                string refTypeName = context.GetTypeName(refEntity, GeneratedType.Entity);
                foreach (var fkField in refRecord.Keys())
                {
                    string fkClrType = context.ClrFieldType(fkField.FieldType, true);
                    WriteFieldAnnotations(field);
                    codeWriter
                        .WriteLine($"public {fkClrType} {field.Name}{fkField.Name} {{ get; set; }}");
                }
                codeWriter.WriteLine($"public {refTypeName} {field.Name} {{ get; set; }}")
                    .WriteLine();
            }
            else if (refEntity is EnumEntity refEnum)
            {
                string refTypeName = context.GetTypeName(refEntity, GeneratedType.Entity);
                WriteFieldAnnotations(field);
                codeWriter
                    .WriteLine($"public long {field.Name}Id {{ get; set; }}")
                    .WriteLine($"public {refTypeName} {field.Name} {{ get; set; }}");
            }
        }

    }
}