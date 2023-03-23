using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal partial class Generator
    {
        private CodeGeneratorContext context;
        private Service service;
        private SchemaToClrConverter converter = new SchemaToClrConverter();

        public Generator(Service service, CodeGeneratorContext codeWriterProvider)
        {
            this.context = codeWriterProvider;
            this.service = service;
        }

        public void Generate()
        {
            GenerateEntities();
            GenerateRepository();
        }

        public void GenerateEntities()
        {
            foreach (var record in converter.Convert(service.Schema))
            {
                GenerateEntity(record);
            }

            foreach (var entity in service.Schema.Entities)
            {
                if (entity is RecordEntity r)
                {
                    GenerateConverter(r);
                }
                else if (entity is EnumEntity e)
                {
                    GenerateEnum(e);
                    GenerateConverter(e);
                    // GenerateEntity(e.BackingRecord__Hack());
                }
            }

        }

        void GenerateEntity(ClrRecord record)
        {
            var writer1 = context.GetWriter(record.Source, RecordFile.Entity);
            var gen1 = new RecordClassWriter(record, service.Schema, writer1, context);
            gen1.Generate();
        }

        void GenerateConverter(RecordEntity record)
        {
            var codeWriter = context.GetWriter(record, RecordFile.Converter);
            var convWriter = new RecordConverterWriter(record, service, codeWriter, context);
            convWriter.Generate();
        }

        private void GenerateConverter(EnumEntity e)
        {
            var writer = context.GetWriter(e, EnumFile.Converter);
            new EnumConverterWriter(e, service, writer, context).Generate();
        }

        void GenerateEnum(EnumEntity enumEntity)
        {
            // Write Enum file
            using var codeWriter = context.GetWriter(enumEntity, EnumFile.Enum);
            var generator = new EnumWriter(service.Schema, enumEntity, codeWriter, context);
            generator.Generate();
        }

        void GenerateRepository()
        {
            using var ifaceCW = context.GetWriter(context.RepositoryInterfaceName);
            var ifaceGen = new RepositoryInterfaceWriter(service, ifaceCW, context);
            ifaceGen.Generate();

            using var classCW = context.GetWriter(context.RepositoryClassName);
            var classGen = new RepositoryClassWriter(service, classCW, context);
            classGen.Generate();

            using var serviceCW = context.GetWriter(context.ServiceClassName);
            var serviceGen = new ServiceClassWriter(service, serviceCW, context);
            serviceGen.Generate();
        }
    }
}