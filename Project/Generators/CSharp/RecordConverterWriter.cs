using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;
using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    class RecordConverterWriter : ClassWriter
    {
        private Service service;
        private CodeGeneratorContext context;

        public RecordConverterWriter(RecordEntity record,
            Service service,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(codeWriter)
        {
            this.service = service;
            this.context = context;
            this.Record = record;

            Imports.Add("using Grpc = global::Grpc.Core;");
            Imports.Add($"using Proto = global::{Common.ProtocOutputNamespace};");
            Imports.Add("using System.ComponentModel.DataAnnotations;");
            Imports.Add("using System.Globalization;");
        }

        RecordEntity Record { get; set; }

        protected override string TargetTypeName => context.MapToDotNetType(Record, RecordFile.Converter);

        protected override string Namespace => service.Schema.Configuration.CSharpNamespace;

        protected override void WriteMethods()
        {
            base.WriteMethods();
            Write_ToMessage();
            codeWriter.WriteLine();
            Write_ToEntity();
        }

        string GetConverterClassName(FieldType fieldType)
        {
            Entity entity = context.GetEntity(fieldType);
            return entity.EntityType switch
            {
                EntityType.Enum => context.MapToDotNetType((EnumEntity)entity, EnumFile.Converter),
                _ => context.MapToDotNetType((RecordEntity)entity, RecordFile.Converter)
            };
        }

        protected override void WriteFields()
        {
            base.WriteFields();
            codeWriter.WriteLine($"const string DateFormatString = \"{Common.DateTimeFormatString}\";");

            HashSet<string> converterClassNames = new HashSet<string>();
            foreach (var field in Record.Fields)
            {
                if (field.FieldType is FieldType.Entity)
                {
                    var className = GetConverterClassName(field.FieldType);
                    converterClassNames.Add(className);
                }
                else if (field.FieldType is FieldType.List ftl && ftl.ItemType is FieldType.Entity)
                {
                    var className = GetConverterClassName(ftl.ItemType);
                    converterClassNames.Add(className);
                }
            }

            foreach (string className in converterClassNames)
            {
                codeWriter.WriteLine($"private static readonly {className} {className.ToCamelCase()} = new();");
            }

            codeWriter.WriteLine();
        }

        void Write_ToMessage()
        {
            var recClassName = context.MapToDotNetType(Record, RecordFile.Entity);
            using (codeWriter.CurlyBracketScope($"public Proto::{Record.Name} Convert({recClassName} entity)"))
            {
                codeWriter.WriteLines(
                    $"ArgumentNullException.ThrowIfNull(entity);",
                    "",
                    $"Proto::{Record.Name} message = new();"
                );

                foreach (var field in Record.Fields)
                {
                    string lhs = $"message.{field.Name}";
                    string rhs = $"entity.{field.Name}";

                    if (field.FieldType.IsNumericPrimitive())
                    {
                        if (field.IsMandatory())
                        {
                            codeWriter.WriteLine($"{lhs} = {rhs};");
                        }
                        else
                        {
                            using (codeWriter.CurlyBracketScope($"if ({rhs}.HasValue)"))
                            {
                                codeWriter.WriteLine($"{lhs} = {rhs}.Value;");
                            }
                        }
                    }
                    else if (field.FieldType.IsSimpleAssignable())
                    {
                        using (codeWriter.CurlyBracketScope($"if (null != {rhs})"))
                        {
                            codeWriter.WriteLine($"{lhs} = {rhs};");
                        }
                    }
                    else if (field is DateTimeField)
                    {

                        if (field.IsMandatory())
                        {
                            codeWriter.WriteLine($"{lhs} = {rhs}.ToString(DateFormatString);");
                        }
                        else
                        {
                            using (codeWriter.CurlyBracketScope($"if (null != {rhs})"))
                            {
                                codeWriter.WriteLine($"{lhs} = {rhs}.Value.ToString(DateFormatString);");
                            }
                        }
                    }
                    else if (field is ReferenceField r)
                    {
                        var entity = context.GetEntity(r.FieldType);
                        var converterClassName = GetConverterClassName(r.FieldType);
                        using (var _ = codeWriter.CurlyBracketScope($"if (null != {rhs})"))
                        {
                            codeWriter.WriteLine($"{lhs} = {converterClassName.ToCamelCase()}.Convert({rhs});");
                            codeWriter.WriteLine($"// TODO: Id not set");
                        }
                    }
                    else if (field is ListField lf)
                    {
                        if (lf.FieldType.ItemType.IsSimpleAssignable())
                        {
                            string itemClrType = context.MapToDotNetType(lf.FieldType.ItemType);
                            codeWriter.WriteLine($"{lhs}.AddRange({rhs});");
                        }
                        else if (lf.FieldType.ItemType is FieldType.Entity ftEnt)
                        {
                            var entity = context.GetEntity(ftEnt);
                            var converterClassName = GetConverterClassName(ftEnt);
                            codeWriter.WriteLines(
                                $"{lhs}.AddRange(",
                                $"      {rhs}.Select(item => {converterClassName.ToCamelCase()}.Convert(item))",
                                ");"
                            );
                        }
                    }
                }
                codeWriter.WriteLine("return message;");
            }
        }

        void Write_ToEntity()
        {
            var recClassName = context.MapToDotNetType(Record, RecordFile.Entity);
            using (codeWriter.CurlyBracketScope($"public {recClassName} Convert(Proto::{Record.Name} message)"))
            {
                codeWriter.WriteLine($"{recClassName} entity = new();");
                foreach (var field in Record.Fields)
                {
                    string lhs = $"entity.{field.Name}";
                    string rhs = $"message.{field.Name}";

                    if (field.FieldType.IsNumericPrimitive())
                    {
                        // Grpc Ints and Floats are never nullable "?"
                        codeWriter.WriteLine($"{lhs} = {rhs};");
                    }
                    else if (field.FieldType.IsSimpleAssignable())
                    {
                        using (codeWriter.CurlyBracketScope($"if (null != {rhs})"))
                        {
                            codeWriter.WriteLine($"{lhs} = {rhs};");
                        }
                    }
                    else if (field is DateTimeField)
                    {
                        using (codeWriter.CurlyBracketScope($"if (null != {rhs} && DateTime.TryParseExact({rhs}, DateFormatString, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime {field.Name.ToCamelCase()}))"))
                        {
                            codeWriter.WriteLine($"{lhs} = {field.Name.ToCamelCase()};");
                        }
                    }
                    else if (field is ReferenceField r)
                    {
                        using (var _ = codeWriter.CurlyBracketScope($"if (null != {rhs})"))
                        {
                            var converterClassName = GetConverterClassName(r.FieldType);
                            codeWriter.WriteLine($"{lhs} = {converterClassName.ToCamelCase()}.Convert({rhs});");
                        }
                    }
                    else if (field is ListField lf)
                    {
                        if (lf.FieldType.ItemType.IsSimpleAssignable())
                        {
                            string clrType = context.MapToDotNetType(lf.FieldType.ItemType);
                            codeWriter.WriteLine($"{lhs} = new List<{clrType}>({rhs});");
                        }
                        else
                        {
                            codeWriter.WriteLine($"// TODO: Assign {field.Name} = {lhs}");
                        }
                    }
                }
                codeWriter.WriteLine("return entity;");
            }
        }
    }
}