namespace Sharara.EntityCodeGen.Generators
{
    class CodeWriter : IDisposable
    {
        const string PaddingIncrement = "    ";

        private bool disposed;
        private TextWriter writer;
        private string currPadding = "";
        private bool currLineIsEmpty = true;


        public CodeWriter(TextWriter writer)
        {
            this.writer = writer;
        }

        public CodeWriter Write(string content)
        {
            ThrowIfDisposed();
            if (currLineIsEmpty)
            {
                writer.Write(currPadding);
                currLineIsEmpty = false;
            }
            writer.Write(content);

            return this;
        }

        public CodeWriter WriteLine()
        {
            ThrowIfDisposed();
            writer.WriteLine();
            currLineIsEmpty = true;
            return this;
        }

        public CodeWriter WriteLine(string line)
        {
            this.Write(line);
            this.WriteLine();
            return this;
        }

        public CodeWriter WriteLines(params string[] lines)
        {
            foreach (var line in lines)
            {
                WriteLine(line);
            }
            return this;
        }

        public CodeWriter Flush()
        {
            ThrowIfDisposed();
            writer.Flush();
            return this;
        }

        public CodeWriter Indent()
        {
            ThrowIfDisposed();
            currPadding += PaddingIncrement;
            return this;
        }

        public CodeWriter UnIndent()
        {
            ThrowIfDisposed();
            int len = currPadding.Length - PaddingIncrement.Length;
            if (len <= 0)
            {
                currPadding = "";
            }
            else
            {
                currPadding = currPadding.Substring(0, len);
            }

            return this;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                writer.Flush();
                writer.Dispose();
                disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("TextWriter");
            }
        }

        public IDisposable CurlyBracketScope(string start, bool bracketOnNewLine = true)
        {
            var arg = bracketOnNewLine ?
                new string[] { start, "{" } :
                new string[] { start + "{" };

            return new Scope(this, arg, new string[] { "}" });
        }

        public class Scope : IDisposable
        {
            private CodeWriter cw;
            private IEnumerable<string> closing;

            public Scope(CodeWriter cw, IEnumerable<string> opening, IEnumerable<string> closing)
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
    }
}