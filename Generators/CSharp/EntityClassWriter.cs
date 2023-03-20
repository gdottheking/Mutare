using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal class EntityClassWriter : ClassWriter, IFieldVisitor
    {
        private Schema schema;
        private CodeGeneratorContext context;
        private readonly RecordEntity record;

        public EntityClassWriter(RecordEntity record,
            Schema definition,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(codeWriter)
        {
            this.record = record;
            this.schema = definition;
            this.context = context;

            this.Imports.Add("using System.ComponentModel.DataAnnotations;");
            this.Imports.Add("using System.ComponentModel.DataAnnotations.Schema;");
        }

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

        void WriteClrField(Field field)
        {
            WriteField(field, context.MapToClrTypeName(field.FieldType));
        }

        public void VisitStringField(StringField field)
        {
            WriteClrField(field);
        }

        public void VisitInt64Field(Int64Field field)
        {
            WriteClrField(field);
        }

        public void VisitInt32Field(Int32Field field)
        {
            WriteClrField(field);
        }

        public void VisitFloat64Field(Float64Field field)
        {
            WriteClrField(field);

        }

        public void VisitDateTimeField(DateTimeField field)
        {
            WriteClrField(field);

        }

        public void VisitReferenceField(ReferenceField field)
        {
            Entity refEntity;
            if (field.FieldType is FieldType.EntityNameRef nameRef)
            {
                if (!schema.HasEntityName(nameRef.EntityName))
                {
                    throw new InvalidOperationException($"Unknown entity: {nameRef.EntityName}");
                }
                refEntity = schema.GetEntityByName(nameRef.EntityName);
            }
            else if (field.FieldType is FieldType.EntityRef ety)
            {
                refEntity = ety.Entity;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (refEntity is RecordEntity refRecord)
            {
                string refTypeName = context.GetTypeName(refEntity, GeneratedType.Entity);
                foreach (var fkField in refRecord.Keys())
                {
                    string fkClrType = context.MapToClrTypeName(fkField.FieldType, true);
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

        public void VisitListField(ListField listField)
        {
            WriteField(listField, context.MapToClrTypeName(listField.FieldType));
        }
    }
}