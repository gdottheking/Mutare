using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;
using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    class EnumConverterWriter : ClassWriter
    {
        private Service service;
        private CodeGeneratorContext context;

        public EnumConverterWriter(EnumEntity enumEntity,
            Service service,
            CodeWriter codeWriter,
            CodeGeneratorContext context)
            : base(codeWriter)
        {
            this.service = service;
            this.context = context;
            this.Enum = enumEntity;

            Imports.Add("using Grpc = global::Grpc.Core;");
            Imports.Add($"using Proto = global::{Common.ProtocOutputNamespace};");
            Imports.Add("using System.ComponentModel.DataAnnotations;");
            Imports.Add("using System.Globalization;");
        }

        EnumEntity Enum { get; set; }

        protected override string ClassKeyword => "class";

        protected override string OutputTypeName => context.GetTypeName(Enum, GeneratedType.Converter);

        protected override string Namespace => service.Schema.Configuration.CSharpNamespace;

        protected override void WriteMethods()
        {
            base.WriteMethods();
            Write_ToMessage();
            codeWriter.WriteLine();
            Write_ToEntity();
        }

        void Write_ToMessage()
        {
            var enumName = context.GetTypeName(Enum, GeneratedType.Enum);
            using (codeWriter.CurlyBracketScope($"public Proto::{Enum.Name} Convert({enumName} value)"))
            {
                using (codeWriter.CurlyBracketScope("return value switch "))
                {
                    for (int i = 0; i < Enum.Values.Count; i++)
                    {
                        EnumValue? val = Enum.Values[i];
                        codeWriter.WriteLine($"{enumName}.{val.Name} => Proto::{Enum.Name}.{val.Name},");
                    }
                    codeWriter.WriteLine($"_ => (Proto::{Enum.Name}) value");
                }
                codeWriter.WriteLine(";");
            }
        }

        void Write_ToEntity()
        {
            var enumName = context.GetTypeName(Enum, GeneratedType.Enum);

            using (codeWriter.CurlyBracketScope($"public {enumName} Convert(Proto::{Enum.Name} value)"))
            {
                using (codeWriter.CurlyBracketScope("return value switch "))
                {
                    for (int i = 0; i < Enum.Values.Count; i++)
                    {
                        EnumValue? val = Enum.Values[i];
                        codeWriter.WriteLine($"Proto::{Enum.Name}.{val.Name} => {enumName}.{val.Name},");
                    }
                    codeWriter.WriteLine($"_ => ({enumName}) value");
                }
                codeWriter.WriteLine(";");
            }
        }

    }

}