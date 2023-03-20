using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;

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
            this.Imports.Add("using System.Text.RegularExpressions;");
        }

        protected EntityClassWriter Indent()
        {
            codeWriter.Indent();
            return this;
        }

        protected EntityClassWriter UnIndent()
        {
            codeWriter.UnIndent();
            return this;
        }

        protected EntityClassWriter WriteLines(params string[] lines)
        {
            codeWriter.WriteLines(lines);
            return this;
        }

        protected EntityClassWriter WriteLine(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                codeWriter.WriteLine();
            }
            return this;
        }

        protected EntityClassWriter WriteLine(string line)
        {
            codeWriter.WriteLine(line);
            return this;
        }

        protected override string OutputTypeName => context.GetTypeName(record, GeneratedType.Entity);

        protected override string Namespace => schema.Configuration.CSharpNamespace;

        protected override string? Implements => "IValidatableObject";

        protected override void WriteFields()
        {
            WriteLines(
                $"const string MinLengthErrMsg = " + "\"Minimum Length Violation\";",
                $"const string MaxLengthErrMsg = " + "\"Maximum Length Violation\";",
                $"const string RequiredValErrMsg = " + "\"Required value not set\";",
                $"const string RegexErrMsg = " + "\"Value is not in expected pattern\";",
                ""
            );

            WriteValidationConstants();
            WriteLine();

            foreach (var field in record.fields)
            {
                field.Accept(this);
            }
        }

        protected override void WriteMethods()
        {
            WriteLine();
            WriteValidateMethod();
        }

        public void VisitField(Field field)
        {
            throw new NotImplementedException();
        }

        private void WriteFieldAnnotations(Field field)
        {
            if (field.IsKey)
            {
                WriteLine($"[Key]");
            }

            if (field.Required)
            {
                WriteLine($"[Required]");
            }

            if (field.CheckOnUpdate)
            {
                WriteLine($"[ConcurrencyCheck]");
            }
        }

        private void WriteField(Field field, string clrType)
        {
            WriteFieldAnnotations(field);
            WriteLine($"public {clrType} {field.Name} {{get; set;}}");
            WriteLine();
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
                WriteLine($"public {refTypeName} {field.Name} {{ get; set; }}")
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

        public void WriteValidateMethod()
        {
            WriteLines(
                "public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)",
                "{"
            ).Indent();

            WriteLine("var result = new List<ValidationResult>();");

            foreach (var field in record.fields)
            {
                if (field is StringField strf)
                {
                    WriteStringFieldValidation(strf);
                }
            }

            WriteLine("return result;");
            UnIndent().WriteLine("}");
        }

        private void WriteValidationConstants()
        {
            foreach (var field in record.fields)
            {
                if (field is StringField strf)
                {
                    if (strf.MinLength.HasValue)
                    {
                        WriteLine($"const int {strf.Name}{nameof(strf.MinLength)} = {strf.MinLength};");
                    }
                    if (strf.MaxLength.HasValue)
                    {
                        WriteLine($"const int {strf.Name}{nameof(strf.MaxLength)} = {strf.MaxLength};");
                    }
                    if (!string.IsNullOrWhiteSpace(strf.RegexPattern)) {
                        var regex = "\"" + strf.RegexPattern + "\"";
                        WriteLine($"static readonly Regex {strf.Name}Regex = new Regex({regex});");
                    }
                }
            }
        }

        private void WriteStringFieldValidation(StringField strf)
        {
            if (strf.Required)
            {
                var test = $"null == {strf.Name}";
                using (IfStat(test))
                {
                    WriteLines(
                        $"result.Add(new ValidationResult(RequiredValErrMsg, new string[]{{ nameof({strf.Name}) }} ));"
                    );
                }
            }

            if (strf.MinLength.HasValue)
            {
                var test = $"null != {strf.Name} && {strf.Name}.Length < {strf.Name}{nameof(strf.MinLength)}";
                using (IfStat(test))
                {
                    WriteLines(
                        $"result.Add(new ValidationResult(MinLengthErrMsg, new string[]{{ nameof({strf.Name}) }} ));"
                    );
                }
            }

            if (strf.MaxLength.HasValue)
            {

                var test = $"null != {strf.Name} && {strf.Name}.Length > {strf.Name}{nameof(strf.MaxLength)}";
                using (IfStat(test))
                {
                    WriteLines(
                        $"result.Add(new ValidationResult(MaxLengthErrMsg, new string[]{{ nameof({strf.Name}) }} ));"
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(strf.RegexPattern))
            {
                var test = $"!{strf.Name}Regex.IsMatch({strf.Name})";
                using (IfStat(test))
                {
                    WriteLines(
                        $"result.Add(new ValidationResult(RegexErrMsg, new string[]{{ nameof({strf.Name}) }} ));"
                    );
                }
            }
        }
    }
}