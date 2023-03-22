using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal class DatabaseContextWriter : ClassWriter
    {
        private Schema schema;
        private CodeGeneratorContext codeWriterProvider;

        public DatabaseContextWriter(Schema definition,
            CodeWriter codeWriter,
            CodeGeneratorContext codeWriterProvider)
            : base(codeWriter)
        {
            this.schema = definition;
            this.codeWriterProvider = codeWriterProvider;
            Imports.Add("using System.Data;");
            Imports.Add("using System.Diagnostics.CodeAnalysis;");
            Imports.Add("using Microsoft.EntityFrameworkCore;");
        }

        protected override string OutputTypeName => "DatabaseContext";

        protected override string Implements => "DbContext";

        protected override string Namespace => schema.Configuration.CSharpNamespace;

        protected override void WriteFields()
        {
            foreach (var entity in schema.Entities)
            {
                if (entity is RecordEntity rec)
                {
                    WriteDbSet(rec);
                }
                // else if (entity is EnumEntity ene)
                // {
                //     WriteDbSet(ene);
                // }
            }
        }

        protected override void WriteMethods()
        {
            WriteOnConfiguringMethod();
            WriteMethodOnModelCreating();
        }

        protected override void WriteConstructor()
        {
            // Constructor
            codeWriter.WriteLine($"public {OutputTypeName}([NotNullAttribute] DbContextOptions options)")
                .Indent()
                .WriteLine($": base(options) {{ }}")
                .UnIndent()
                .WriteLine()
                .WriteLine($"public {OutputTypeName}()")
                .WriteLine("{")
                .WriteLine("}");
        }

        private void WriteOnConfiguringMethod()
        {
            codeWriter.WriteLine()
            .WriteLine("protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {")
            .Indent()
            .WriteLine("base.OnConfiguring(optionsBuilder);")
            .UnIndent()
            .WriteLine("}")
            .WriteLine();
        }

        private void WriteMethodOnModelCreating()
        {
            codeWriter.WriteLine("protected override void OnModelCreating(ModelBuilder builder) {")
                            .Indent()
                            .WriteLine("base.OnModelCreating(builder);")
                            .UnIndent()
                            .WriteLine("}")
                            .WriteLine();
        }

        private void WriteDbSet(RecordEntity recEntity)
        {
            string typeName = codeWriterProvider.MapToDotNetType(recEntity, RecordFile.Entity);
            codeWriter.WriteLine($"public DbSet<{typeName}> {recEntity.Name} {{ get; set; }}")
            .WriteLine();
        }

        private void WriteDbSet(EnumEntity enumEntity)
        {
            string typeName = codeWriterProvider.MapToDotNetType(enumEntity, EnumFile.Entity);
            codeWriter.WriteLine($"public DbSet<{typeName}> {enumEntity.Name} {{ get; set; }}")
            .WriteLine();
        }
    }
}