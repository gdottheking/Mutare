
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

        protected enum CSharpTargetType {
            Class,
            Interface,
            Enum
        }

        protected virtual CSharpTargetType TargetType { get; } = CSharpTargetType.Class;

        protected List<string> Imports { get; } = new List<string>();

        protected abstract string TargetTypeName { get; }

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
            string output = $"public {TargetType.ToString().ToLower()} {TargetTypeName}";
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

        protected IDisposable IfStat(string condition)
        {
            return codeWriter.CurlyBracketScope($"if ({condition})");
        }
    }
}
