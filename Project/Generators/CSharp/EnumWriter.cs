using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    class EnumWriter : ClassWriter
    {
        private readonly EnumEntity enumEntity;
        private readonly CodeGeneratorContext helper;
        private readonly Schema schema;

        public EnumWriter(Schema schema, EnumEntity enumEntity, CodeWriter codeWriter, CodeGeneratorContext helper)
            : base(codeWriter)
        {
            this.schema = schema;
            this.enumEntity = enumEntity;
            this.helper = helper;
        }

        protected override string OutputTypeName => helper.MapToDotNetType(enumEntity, EnumFile.Enum);

        protected override string Namespace => schema.Configuration.CSharpNamespace;

        protected override string ClassKeyword => "enum";

        protected override string? Implements => null;

        protected override void WriteBody()
        {
            for (int i = 0; i < enumEntity.Values.Count; i++)
            {
                var value = enumEntity.Values[i];
                codeWriter.Write($"{value.Name} = {value.Value}");
                if (i < enumEntity.Values.Count - 1)
                {
                    codeWriter.WriteLine(",");
                }
            }
            codeWriter.WriteLine();
        }

    }

}