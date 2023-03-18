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

        public CodeWriter WriteLine(string content)
        {
            this.Write(content);
            this.WriteLine();
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
    }
}