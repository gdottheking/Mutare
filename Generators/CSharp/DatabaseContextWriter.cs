using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal class DbContextGen : ClassWriter
    {
        private Schema schema;
        private ICodeGeneratorContext codeWriterProvider;

        static readonly string[] imports = new string[] {
            "using System;",
            "using System.Data;",
            "using System.Data.Entity;",
            "using System.Diagnostics.CodeAnalysis;"
        };

        public DbContextGen(Schema definition, CodeWriter codeWriter, ICodeGeneratorContext codeWriterProvider)
            :base(codeWriter)
        {
            this.schema = definition;
            this.codeWriterProvider = codeWriterProvider;
        }

        protected override IEnumerable<string> Imports => imports;

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
                else if (entity is EnumEntity ene)
                {
                    WriteDbSet(ene);
                }
            }
        }

        protected override void WriteMethods()
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

            codeWriter.WriteLine()
                .WriteLine("protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {")
                .Indent()
                .WriteLine("base.OnConfiguring(optionsBuilder);")
                .UnIndent()
                .WriteLine("}")
                .WriteLine();

            codeWriter.WriteLine("protected override void OnModelCreating(ModelBuilder builder) {")
                .Indent()
                .WriteLine("base.OnModelCreating(builder);")
                .UnIndent()
                .WriteLine("}")
                .WriteLine();
        }

        private void WriteDbSet(RecordEntity rec)
        {
            string typeName = codeWriterProvider.GetTypeName(rec, GeneratedType.Entity);
            codeWriter.WriteLine($"public DbSet<{typeName}> {rec.Name} {{ get; set; }}")
            .WriteLine();
        }

        private void WriteDbSet(EnumEntity ene)
        {
            string typeName = codeWriterProvider.GetTypeName(ene, GeneratedType.Entity);
            codeWriter.WriteLine($"public DbSet<{typeName}> {ene.Name} {{ get; set; }}")
            .WriteLine();
        }
    }
}