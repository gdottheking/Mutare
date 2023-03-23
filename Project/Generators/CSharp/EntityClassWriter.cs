using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;
using Sharara.EntityCodeGen.Core.Fields.Types;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal class RecordClassWriter : ClassWriter
    {
        private Schema schema;
        private CodeGeneratorContext context;
        private readonly ClrRecord record;
        private readonly ValidationWriter validationWriter;
        private readonly ClrTypeMapper typeMapper = new ClrTypeMapper();

        public RecordClassWriter(ClrRecord record,
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

        protected RecordClassWriter Indent()
        {
            codeWriter.Indent();
            return this;
        }

        protected RecordClassWriter UnIndent()
        {
            codeWriter.UnIndent();
            return this;
        }

        protected RecordClassWriter WriteLines(params string[] lines)
        {
            codeWriter.WriteLines(lines);
            return this;
        }

        protected RecordClassWriter WriteLine(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                codeWriter.WriteLine();
            }
            return this;
        }

        protected RecordClassWriter WriteLine(string line)
        {
            codeWriter.WriteLine(line);
            return this;
        }

        protected override string TargetTypeName => record.ClassName;

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

            foreach (var prop in record.Properties.OrderBy(prop => prop.Name))
            {
                WriteProp(prop);
                WriteLine();
            }
        }

        private void WriteProp(IClrProperty prop)
        {
            if (prop is ClrProperty standardProp)
            {
                WriteFieldAnnotations(standardProp.Source);
            }
            else if (prop is ClrShadowProperty shadowProperty)
            {
                WriteFieldAnnotations(shadowProperty.ShadowField.Pointer);
            }

            WriteLine($"public {prop.ClrType} {prop.Name} {{get; set;}}");
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

        class ValidationWriter : IFieldVisitor
        {
            private readonly RecordClassWriter parent;

            public ValidationWriter(RecordClassWriter parent)
            {
                this.parent = parent;
            }

            Field FieldFromProp(IClrProperty prop)
            {
                if (prop is ClrProperty standard)
                {
                    return standard.Source;
                }
                else if (prop is ClrShadowProperty shadow)
                {
                    // TODO Hacky hack!
                    return shadow.ShadowField.Target with { Name = prop.Name, IsRequired = !prop.DefaultsToNull };
                }
                throw new NotImplementedException();
            }


            public void WriteMethods()
            {
                foreach (var prop in parent.record.Properties)
                {
                    FieldFromProp(prop).Accept(this);
                }

                var methodDecl = "public IEnumerable<ValidationResult> "
                    + "Validate(ValidationContext validationContext)";

                using (parent.codeWriter.CurlyBracketScope(methodDecl))
                {
                    parent.WriteLine("var result = new List<ValidationResult>();");

                    foreach (var prop in parent.record.Properties)
                    {
                        parent.WriteLine($"result.AddRange(Validate{prop.Name}(validationContext));");
                    }

                    parent.WriteLine("return result;");
                }
            }

            public void WriteFields()
            {
                foreach (var prop in parent.record.Properties)
                {
                    var field = FieldFromProp(prop);
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