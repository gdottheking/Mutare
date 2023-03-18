
using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    interface ICodeGeneratorContext
    {
        string OutputFolder {get;}

        CodeWriter GetWriter(Entity entity, GeneratedType type);

        CodeWriter GetWriter(GeneratedType type);

        string GetTypeName(Entity entity, GeneratedType type);

        string GetTypeName(GeneratedType type);

        string ClrFieldType(FieldType fieldType, bool forceNullable = false);

    }
}
