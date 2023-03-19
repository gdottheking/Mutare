using Sharara.EntityCodeGen.Core;

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

        public void Generate() {
            GenerateEntities();
            GenerateRepository();
        }

        public void GenerateEntities()
        {
            foreach (var entity in service.Schema.Entities)
            {
                if(entity is RecordEntity r){
                    GenerateEntity(r);
                } else if(entity is EnumEntity e) {
                    GenerateEnum(e);
                    GenerateEntity(e.BackingRecord__Hack());
                }
            }
        }

        void GenerateEntity(RecordEntity entity)
        {
            var writer = context.GetWriter(entity, GeneratedType.Entity);
            var gen = new EntityClassWriter(entity, service.Schema, writer, context);
            gen.Generate();
        }

        void GenerateEnum(EnumEntity enumEntity)
        {
            // Write Enum file
            using var codeWriter = context.GetWriter(enumEntity, GeneratedType.Enum);
            var generator = new EnumWriter(service.Schema, enumEntity, codeWriter, context);
            generator.Generate();
        }

        void GenerateRepository() {
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