using System.Runtime.CompilerServices;

namespace Sharara.EntityCodeGen.Core
{
    public class SchemaException : System.Exception
    {
        public SchemaException() { }
        public SchemaException(string message) : base(message) { }
        public SchemaException(string message, System.Exception inner) : base(message, inner) { }
        protected SchemaException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}