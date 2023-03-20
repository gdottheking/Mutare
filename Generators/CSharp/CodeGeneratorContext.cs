using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal class CodeGeneratorContext
    {

        public CodeGeneratorContext(string outputFolder)
        {
            OutputFolder = outputFolder;
        }

        public string OutputFolder { get; }

        public string GetTypeName(GeneratedType type)
        {
            return type switch
            {
                GeneratedType.RepoInterface => "IRepository",
                GeneratedType.RepoClass => "Repository",
                GeneratedType.Service => "Service",
                _ => throw new InvalidOperationException()
            };
        }

        public string GetTypeName(Entity entity, GeneratedType type)
        {
            ArgumentNullException.ThrowIfNull(entity.Name);
            return type switch
            {
                GeneratedType.None => entity.Name,
                GeneratedType.Converter => entity.Name + "Converter",
                GeneratedType.Entity => entity.Name + "Entity",
                GeneratedType.Enum => entity.Name + "Enum",
                _ => throw new InvalidOperationException()
            };
        }

        public CodeWriter GetWriter(GeneratedType fileType)
        {
            var className = GetTypeName(fileType);
            return GetWriter(className);
        }

        public CodeWriter GetWriter(Entity entity, GeneratedType fileType)
        {
            var className = GetTypeName(entity, fileType);
            return GetWriter(className);
        }

        CodeWriter GetWriter(string className)
        {
            var fileName = $"{OutputFolder}/{className}.cs";
            var writer = File.CreateText(fileName);
            return new CodeWriter(writer);
        }

        public string MapToClrTypeName(FieldType fieldType, bool forceNullable = false)
        {
            Func<string, string> Coerce = (clrType) => forceNullable ? clrType += "?" : clrType;

            return fieldType switch
            {
                FieldType.DateTime => Coerce("DateTime"),
                FieldType.Float64 => Coerce("double"),
                FieldType.Int64 => Coerce("long"),
                FieldType.Int32 => Coerce("int"),
                FieldType.String => "string",
                FieldType.List l => $"IEnumerable<{MapToClrTypeName(l.ItemType)}>",
                FieldType.Void => "void",
                FieldType.EntityRef entyRef => GetTypeName(entyRef.Entity, GeneratedType.Entity),
                FieldType.EntityNameRef nameRef => GetTypeName(nameRef.ResolvedEntity!, GeneratedType.Entity),
                _ => throw new NotImplementedException($"FieldType {fieldType} does not have a matching clrType")
            };
        }

        public string ClrDeclString(OperationInfo op)
        {
            string @params = string.Join(", ", op.Arguments.Select(ClrDeclString));
            string returnType = MapToClrTypeName(op.ReturnType);
            returnType = returnType.Equals("void") ? "Task" : $"Task<{returnType}>";
            return $"{returnType} {op.Name}Async({@params})";
        }

        public string ClrDeclString(Argument arg)
        {
            string typeName = MapToClrTypeName(arg.Type);
            return $"{typeName} {arg.Name}";
        }

    }
}
