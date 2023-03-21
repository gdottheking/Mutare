using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal partial class Generator
    {
        private CodeGeneratorContext context;
        private Service service;

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
            foreach (var entity in service.Schema.Entities)
            {
                if (entity is RecordEntity r)
                {
                    GenerateEntity(r);
                    GenerateConverter(r);
                }
                else if (entity is EnumEntity e)
                {
                    GenerateEnum(e);
                    GenerateEntity(e.BackingRecord__Hack());
                }
            }
        }

        void GenerateEntity(RecordEntity record)
        {
            var writer1 = context.GetWriter(record, GeneratedType.Entity);
            var gen1 = new EntityClassWriter(record, service.Schema, writer1, context);
            gen1.Generate();


        }

        void GenerateConverter(RecordEntity record)
        {
            var writer2 = context.GetWriter(record, GeneratedType.Converter);
            var gen2 = new RecordConverterWriter(record, service, writer2, context);
            gen2.Generate();
        }

        void GenerateEnum(EnumEntity enumEntity)
        {
            // Write Enum file
            using var codeWriter = context.GetWriter(enumEntity, GeneratedType.Enum);
            var generator = new EnumWriter(service.Schema, enumEntity, codeWriter, context);
            generator.Generate();
        }

        void GenerateRepository()
        {
            using var ifaceCW = context.GetWriter(GeneratedType.RepoInterface);
            var ifaceGen = new RepositoryInterfaceWriter(service, ifaceCW, context);
            ifaceGen.Generate();

            using var classCW = context.GetWriter(GeneratedType.RepoClass);
            var classGen = new RepositoryClassWriter(service, classCW, context);
            classGen.Generate();

            using var serviceCW = context.GetWriter(GeneratedType.Service);
            var serviceGen = new ServiceClassWriter(service, serviceCW, context);
            serviceGen.Generate();
        }
    }
}