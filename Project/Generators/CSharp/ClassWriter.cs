
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

        protected virtual string? Implements => null;

        protected virtual string ClassKeyword { get; } = "class";

        protected List<string> Imports { get; } = new List<string>();

        protected abstract string OutputTypeName { get; }

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
            WriteConstructor();
            WriteMethods();
        }

        protected virtual void WriteImports()
        {
            codeWriter.WriteLines("using System;",
            "using System.Linq;",
            "using System.Collections;",
            "using System.Collections.Generic;",
            "using System.Threading.Tasks;");

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

        protected virtual void WriteConstructor()
        {
        }

        protected virtual void WriteMethods()
        {
        }

        public void Dispose()
        {
            codeWriter.Flush().Dispose();
        }

        protected class ScopeHelper : IDisposable
        {
            private CodeWriter cw;
            private IEnumerable<string> closing;

            public ScopeHelper(CodeWriter cw, IEnumerable<string> opening, IEnumerable<string> closing)
            {
                this.cw = cw;
                this.closing = closing;
                foreach (var line in opening)
                {
                    this.cw.WriteLine(line);
                }
                this.cw.Indent();
            }

            public void Dispose()
            {
                this.cw.UnIndent();
                foreach (var line in closing)
                {
                    this.cw.WriteLine(line);
                }
            }
        }

        protected IDisposable IfStat(string condition)
        {
            var scope = new ScopeHelper(
                codeWriter,
                    new string[] { $"if ({condition})", "{" },
                 new string[] { "}" }
            );

            return scope;
        }
    }
}
