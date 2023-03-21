using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;
using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    class RecordConverterWriter : ClassWriter
    {
        private Service service;
        private CodeGeneratorContext context;
        private readonly RecordEntity record;
        public const string FormatString = "yyyyMMddHHmmsss.zzz";

        public RecordConverterWriter(RecordEntity record,
            Service service,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(codeWriter)
        {
            this.record = record;
            this.service = service;
            this.context = context;
            this.Record = record;

            Imports.Add("using grpc = global::Grpc.Core;");
            Imports.Add($"using proto = global::{Common.ProtocOutputNamespace};");
            Imports.Add("using System.ComponentModel.DataAnnotations;");
            Imports.Add("using System.Globalization;");
        }

        RecordEntity Record { get; set; }

        protected override string ClassKeyword => "class";

        protected override string OutputTypeName => context.GetTypeName(Record, GeneratedType.Converter);

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
            return context.GetTypeName(entity, GeneratedType.Converter);
        }

        protected override void WriteFields()
        {
            base.WriteFields();
            codeWriter.WriteLine($"const string DateFormatString = \"{FormatString}\";");
            foreach (var field in Record.Fields)
            {
                if (field is ReferenceField r)
                {
                    var converterClassName = GetConverterClassName(r.FieldType);
                    codeWriter.WriteLine($"private static readonly {converterClassName} {converterClassName.ToCamelCase()} = new();");
                }
            }
            codeWriter.WriteLine();
        }

        bool IsSimpleAssignable(FieldType fieldType)
        {
            return fieldType is FieldType.String ||
                        fieldType is FieldType.Int64 ||
                        fieldType is FieldType.Int32 ||
                        fieldType is FieldType.Float64;
        }

        void Write_ToMessage()
        {
            var recClassName = context.GetTypeName(Record, GeneratedType.Entity);
            using (codeWriter.CurlyBracketScope($"public proto::{Record.Name} ToMessage({recClassName} entity)"))
            {
                codeWriter.WriteLines(
                    $"ArgumentNullException.ThrowIfNull(entity);",
                    "",
                    $"proto::{Record.Name} message = new();"
                );

                foreach (var field in Record.Fields)
                {
                    string lhs = $"message.{field.Name}";
                    string rhs = $"entity.{field.Name}";

                    if (IsSimpleAssignable(field.FieldType))
                    {
                        using (codeWriter.CurlyBracketScope($"if (null != {rhs})"))
                        {
                            codeWriter.WriteLine($"{lhs} = {rhs};");
                        }
                    }
                    else if (field is DateTimeField)
                    {
                        using (codeWriter.CurlyBracketScope($"if (null != {rhs})"))
                        {
                            codeWriter.WriteLine($"{lhs} = {rhs}.ToString(DateFormatString);");
                        }
                    }
                    else if (field is ReferenceField r)
                    {
                        var entity = context.GetEntity(r.FieldType);
                        if (entity is RecordEntity)
                        {
                            var converterClassName = GetConverterClassName(r.FieldType);
                            codeWriter.WriteLine($"{lhs} = {converterClassName.ToCamelCase()}.ToMessage({rhs});");
                            codeWriter.WriteLine($"// TODO: Id not set");
                        }
                        else if (entity is EnumEntity enumEntity)
                        {
                            var keys = enumEntity.BackingRecord__Hack().Keys();
                            foreach (var key in keys)
                            {
                                codeWriter.WriteLine($"{lhs} = (proto::{entity.Name}) {rhs};");
                            }
                        }
                    }
                    else if (field is ListField lf)
                    {
                        if (IsSimpleAssignable(lf.FieldType.ItemType))
                        {
                            string itemClrType = context.MapToClrTypeName(lf.FieldType.ItemType);
                            codeWriter.WriteLine($"{lhs}.AddRange({rhs});");
                        }
                        else
                        {
                            codeWriter.WriteLine($"// List assignment not implemented {lhs} = {rhs}");
                        }
                    }
                }
                codeWriter.WriteLine("return message;");
            }
        }

        void Write_ToEntity()
        {
            var recClassName = context.GetTypeName(Record, GeneratedType.Entity);
            using (codeWriter.CurlyBracketScope($"public {recClassName} ToEntity(proto::{Record.Name} message)"))
            {
                codeWriter.WriteLine($"{recClassName} entity = new();");
                foreach (var field in Record.Fields)
                {
                    string lhs = $"entity.{field.Name}";
                    string rhs = $"message.{field.Name}";

                    if (field is StringField ||
                        field is Int64Field ||
                        field is Int32Field ||
                        field is Float64Field)
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
                        var converterClassName = GetConverterClassName(r.FieldType);
                        codeWriter.WriteLine($"{lhs} = {converterClassName.ToCamelCase()}.ToEntity({rhs});");
                    }
                    else if (field is ListField lf)
                    {
                        if (IsSimpleAssignable(lf.FieldType.ItemType))
                        {
                            string clrType = context.MapToClrTypeName(lf.FieldType.ItemType);
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