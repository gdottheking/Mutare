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
                else if (entity is EnumEntity ene)
                {
                    WriteDbSet(ene);
                }
            }
        }

        protected override void WriteMethods()
        {
            WriteOnConfiguringMethod();
            WriteMethodOnModelCreating();
            //WriteMethodValidateEntity();
        }

        protected override void WriteConstructor() {
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

        // private void WriteMethodValidateEntity()
        // {
        //     codeWriter.WriteLines(
        //         "protected override DbEntityValidationResult ValidateEntity(",
        //         "   System.Data.Entity.Infrastructure.DbEntityEntry entityEntry,",
        //         "   IDictionary<object, object> items)",
        //         "{")
        //         .Indent();

        //     codeWriter.WriteLine("var result = new DbEntityValidationResult(entityEntry, new List<DbValidationError>());");
        //     codeWriter.WriteLine()
        //         .WriteLine("// TODO: Custom validation here!")
        //         .WriteLine();

        //     codeWriter.WriteLine("if (result.ValidationErrors.Count > 0)")
        //         .WriteLine("{")
        //         .Indent()
        //         .WriteLine("return result;")
        //         .UnIndent()
        //         .WriteLine("}")
        //         .WriteLine("else")
        //         .WriteLine("{")
        //         .Indent()
        //         .WriteLine("return base.ValidateEntity(entityEntry, items);")
        //         .UnIndent()
        //         .WriteLine("}")
        //         .UnIndent()
        //         .WriteLine("}");
        // }

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