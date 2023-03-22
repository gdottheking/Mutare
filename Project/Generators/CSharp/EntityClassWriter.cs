using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal class EntityClassWriter : ClassWriter, IFieldVisitor
    {
        private Schema schema;
        private CodeGeneratorContext context;
        private readonly RecordEntity record;
        private readonly ValidationWriter validationWriter;

        public EntityClassWriter(RecordEntity record,
            Schema definition,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(codeWriter)
        {
            this.record = record;
            this.schema = definition;
            this.context = context;
            this.validationWriter = new ValidationWriter(this);

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

        protected override string OutputTypeName => context.MapToDotNetType(record, RecordFile.Entity);

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

            validationWriter.WriteFields();
            WriteLine();

            foreach (var field in record.Fields.OrderBy(f => f.Name))
            {
                field.Accept(this);
                WriteLine();
            }
        }

        protected override void WriteMethods()
        {
            validationWriter.WriteMethods();
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

            if (field.IsRequired)
            {
                WriteLine($"[Required]");
            }

            if (field.CheckOnUpdate)
            {
                WriteLine($"[ConcurrencyCheck]");
            }
        }

        private void WriteClassProperty(Field field, string clrType, string? overrideName = null)
        {
            string propName = !string.IsNullOrWhiteSpace(overrideName) ? overrideName : field.Name;
            WriteFieldAnnotations(field);
            WriteLine($"public {clrType} {propName} {{get; set;}}");
        }

        void WriteClrField(Field field)
        {
            WriteClassProperty(field, context.MapToDotNetType(field));
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
            Entity refEntity = context.GetEntity(field.FieldType);
            Console.WriteLine($"Writing {field.Name}");

            if (refEntity.EntityType == EntityType.Record)
            {
                RecordEntity targetRec = (RecordEntity)refEntity;
                // Write shadow fields
                var shadowFields = this.record.ShadowFields();
                foreach (var shadow in shadowFields)
                {
                    if (shadow.Target.Owner == targetRec)
                    {
                        string targetClrType = context.MapToDotNetType(shadow.Target.Field.FieldType, true);
                        WriteLine($"public {targetClrType} {shadow.Name} {{get; set;}}");
                    }

                    // Write
                    string refTypeName = context.MapToDotNetType(targetRec, RecordFile.Entity);
                    WriteClassProperty(field, refTypeName);
                }
            }
            else if (refEntity.EntityType == EntityType.Enum)
            {
                EnumEntity refEnum = (EnumEntity)refEntity;
                string enumName = context.MapToDotNetType(refEnum, EnumFile.Enum);
                WriteClassProperty(field, enumName);
            }
        }

        public void VisitListField(ListField listField)
        {
            string propType;
            if (listField.FieldType.IsEnumOrListOfEnums())
            {
                var fte = (FieldType.Entity)(listField.FieldType.ItemType);
                var ent = (EnumEntity) fte.GetEntity();
                var type = context.MapToDotNetType(ent);
                propType = $"IEnumerable<{type}>";
            }
            else
            {
                propType = context.MapToDotNetType(listField.FieldType);
            }

            WriteClassProperty(listField, propType);
        }

        class ValidationWriter : IFieldVisitor
        {
            private readonly EntityClassWriter parent;

            public ValidationWriter(EntityClassWriter parent)
            {
                this.parent = parent;
            }

            public void WriteMethods()
            {
                foreach (var field in parent.record.Fields)
                {
                    field.Accept(this);
                }

                var methodDecl = "public IEnumerable<ValidationResult> "
                    + "Validate(ValidationContext validationContext)";

                using (parent.codeWriter.CurlyBracketScope(methodDecl))
                {
                    parent.WriteLine("var result = new List<ValidationResult>();");

                    foreach (var field in parent.record.Fields)
                    {
                        parent.WriteLine($"result.AddRange(Validate{field.Name}(validationContext));");
                    }

                    parent.WriteLine("return result;");
                }
            }

            public void WriteFields()
            {
                foreach (var field in parent.record.Fields)
                {
                    if (field is StringField strf)
                    {
                        if (strf.MinLength.HasValue)
                        {
                            parent.WriteLine($"const int {strf.Name}{nameof(strf.MinLength)} = {strf.MinLength};");
                        }
                        if (strf.MaxLength.HasValue)
                        {
                            parent.WriteLine($"const int {strf.Name}{nameof(strf.MaxLength)} = {strf.MaxLength};");
                        }
                        if (!string.IsNullOrWhiteSpace(strf.RegexPattern))
                        {
                            var regex = "\"" + strf.RegexPattern + "\"";
                            parent.WriteLine($"static readonly Regex {strf.Name}Regex = new Regex({regex});");
                        }
                    }
                }
            }

            public void VisitDateTimeField(DateTimeField strf)
            {
                var methodDecl = "public IEnumerable<ValidationResult> "
                    + $"Validate{strf.Name}(ValidationContext validationContext)";

                using var _ = parent.codeWriter.CurlyBracketScope(methodDecl);
                parent.WriteLine("var result = new List<ValidationResult>();");
                // TODO: Validation here
                parent.WriteLine("return result;");
            }

            public void VisitStringField(StringField strf)
            {
                var methodDecl = "public IEnumerable<ValidationResult> "
                    + $"Validate{strf.Name}(ValidationContext validationContext)";

                using var _ = parent.codeWriter.CurlyBracketScope(methodDecl);

                parent.WriteLine("var result = new List<ValidationResult>();");

                if (strf.IsRequired)
                {
                    var test = $"null == {strf.Name}";
                    using (parent.IfStat(test))
                    {
                        parent.WriteLines(
                            $"result.Add(new ValidationResult(RequiredValErrMsg, new string[]{{ nameof({strf.Name}) }} ));"
                        );
                    }
                }

                if (strf.MinLength.HasValue)
                {
                    var test = $"null != {strf.Name} && {strf.Name}.Length < {strf.Name}{nameof(strf.MinLength)}";
                    using (parent.IfStat(test))
                    {
                        parent.WriteLines(
                            $"result.Add(new ValidationResult(MinLengthErrMsg, new string[]{{ nameof({strf.Name}) }} ));"
                        );
                    }
                }

                if (strf.MaxLength.HasValue)
                {

                    var test = $"null != {strf.Name} && {strf.Name}.Length > {strf.Name}{nameof(strf.MaxLength)}";
                    using (parent.IfStat(test))
                    {
                        parent.WriteLines(
                            $"result.Add(new ValidationResult(MaxLengthErrMsg, new string[]{{ nameof({strf.Name}) }} ));"
                        );
                    }
                }

                if (!string.IsNullOrWhiteSpace(strf.RegexPattern))
                {
                    var test = $"!{strf.Name}Regex.IsMatch({strf.Name})";
                    using (parent.IfStat(test))
                    {
                        parent.WriteLines(
                            $"result.Add(new ValidationResult(RegexErrMsg, new string[]{{ nameof({strf.Name}) }} ));"
                        );
                    }
                }

                parent.WriteLine("return result;");

            }

            public void VisitField(Field field)
            {
                throw new NotImplementedException();
            }

            public void VisitInt32Field(Int32Field field)
            {
                var methodDecl = "public IEnumerable<ValidationResult> "
                    + $"Validate{field.Name}(ValidationContext validationContext)";

                using var _ = parent.codeWriter.CurlyBracketScope(methodDecl);
                parent.WriteLine("var result = new List<ValidationResult>();");
                // TODO: Validation here
                parent.WriteLine("return result;");
            }

            public void VisitInt64Field(Int64Field field)
            {
                var methodDecl = "public IEnumerable<ValidationResult> "
                    + $"Validate{field.Name}(ValidationContext validationContext)";

                using var _ = parent.codeWriter.CurlyBracketScope(methodDecl);
                parent.WriteLine("var result = new List<ValidationResult>();");
                // TODO: Validation here
                parent.WriteLine("return result;");
            }

            public void VisitFloat64Field(Float64Field field)
            {
                var methodDecl = "public IEnumerable<ValidationResult> "
                    + $"Validate{field.Name}(ValidationContext validationContext)";

                using var _ = parent.codeWriter.CurlyBracketScope(methodDecl);
                parent.WriteLine("var result = new List<ValidationResult>();");
                // TODO: Validation here
                parent.WriteLine("return result;");
            }

            public void VisitReferenceField(ReferenceField field)
            {
                var methodDecl = "public IEnumerable<ValidationResult> "
                    + $"Validate{field.Name}(ValidationContext validationContext)";

                using var _ = parent.codeWriter.CurlyBracketScope(methodDecl);
                parent.WriteLine("var result = new List<ValidationResult>();");
                // TODO: Validation here
                parent.WriteLine("return result;");
            }

            public void VisitListField(ListField field)
            {
                var methodDecl = "public IEnumerable<ValidationResult> "
                    + $"Validate{field.Name}(ValidationContext validationContext)";

                using var _ = parent.codeWriter.CurlyBracketScope(methodDecl);
                parent.WriteLine("var result = new List<ValidationResult>();");
                // TODO: Validation here
                parent.WriteLine("return result;");
            }
        }
    }
}