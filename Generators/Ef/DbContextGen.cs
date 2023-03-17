using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.Ef
{
    internal class DbContextGen : IEntityVisitor, IDisposable
    {
        private CodeWriter codeWriter;
        private Schema definition;

        static readonly string[] Usings = new string[] {
            "using System;",
            "using System.Data;",
            "using System.Data.Entity;",
            "using System.Diagnostics.CodeAnalysis;"
        };

        public DbContextGen(Schema definition, CodeWriter codeWriter)
        {
            this.codeWriter = codeWriter;
            this.definition = definition;
        }

        public void VisitRecord(RecordEntity rec)
        {
            codeWriter.WriteLine($"public DbSet<{rec.Name}> {rec.Name} {{ get; set; }}");
        }


        public void Dispose()
        {
            codeWriter.Dispose();
        }

        public void Generate()
        {
            foreach (var str in Usings)
            {
                codeWriter.WriteLine(str);
            }
            codeWriter.WriteLine();

            codeWriter.WriteLine("public class DatabaseContext : DbContext");
            codeWriter.WriteLine("{");
            codeWriter.Indent();

            foreach (var entity in definition.Entities)
            {
                entity.Accept(this);
            }

            codeWriter.UnIndent();
            codeWriter.WriteLine("}");
            codeWriter.Flush();
        }

        public void VisitDateTimeField(DateTimeField field)
        {
            throw new NotImplementedException();
        }

        public void VisitEnum(EnumEntity entity)
        {
            // do nothing
        }

        public void VisitEnumValue(EnumValue value, int index, int count)
        {
            throw new NotImplementedException();
        }

        public void VisitField(Field field)
        {
            throw new NotImplementedException();
        }

        public void VisitFloat64Field(Float64Field field)
        {
            throw new NotImplementedException();
        }

        public void VisitInt64Field(Int64Field field)
        {
            throw new NotImplementedException();
        }

        public void VisitReferenceField(ReferenceField field)
        {
            throw new NotImplementedException();
        }

        public void VisitStringField(StringField field)
        {
            throw new NotImplementedException();
        }
    }
}