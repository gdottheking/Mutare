namespace Sharara.EntityCodeGen.Generators.CSharp
{
    // Helper for writing C# classes
    internal abstract class ClassWriter : IDisposable
    {
        protected CodeWriter codeWriter;

        public ClassWriter(CodeWriter codeWriter)
        {
            this.codeWriter = codeWriter;
        }

        protected abstract IEnumerable<string> Imports { get; }

        protected abstract string OutputTypeName { get; }

        protected virtual string? Implements => "Object";

        protected virtual string ClassKeyword { get; } = "class";

        protected abstract string Namespace { get; }

        public void Generate()
        {
            WriteImports();
            OpenNamespace();
            OpenClass();
            WriteBody();
            CloseClass();
            CloseNameSpace();
            codeWriter.Flush();
        }

        protected virtual void WriteBody()
        {
            WriteFields();
            WriteMethods();
        }

        protected virtual void WriteImports()
        {
            foreach (var str in Imports)
            {
                codeWriter.WriteLine(str);
            }
        }

        protected virtual void OpenNamespace()
        {
            codeWriter.WriteLine()
                .WriteLine($"namespace {Namespace}")
                .WriteLine("{")
                .Indent();
        }

        protected virtual void CloseNameSpace()
        {
            codeWriter.UnIndent().WriteLine("}");
        }

        protected virtual void CloseClass()
        {
            codeWriter.UnIndent().WriteLine("}");
        }

        protected virtual void OpenClass()
        {
            string output = $"public {ClassKeyword} {OutputTypeName}";
            if (!string.IsNullOrWhiteSpace(Implements))
            {
                output += $": {Implements}";
            }

            codeWriter.WriteLine(output)
                .WriteLine("{")
                .Indent();
        }

        protected virtual void WriteFields()
        {
        }

        protected virtual void WriteMethods()
        {
        }

        public void Dispose()
        {
            codeWriter.Flush().Dispose();
        }
    }
}
