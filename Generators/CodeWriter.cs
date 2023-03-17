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

        public void Write(string content)
        {
            ThrowIfDisposed();
            if (currLineIsEmpty)
            {
                writer.Write(currPadding);
                currLineIsEmpty = false;
            }
            writer.Write(content);
        }

        public void WriteLine()
        {
            ThrowIfDisposed();
            writer.WriteLine();
            currLineIsEmpty = true;
        }

        public void WriteLine(string content)
        {
            this.Write(content);
            this.WriteLine();
        }

        public void Flush()
        {
            ThrowIfDisposed();
            writer.Flush();
        }

        public void Indent()
        {
            ThrowIfDisposed();
            currPadding += PaddingIncrement;
        }

        public void UnIndent()
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