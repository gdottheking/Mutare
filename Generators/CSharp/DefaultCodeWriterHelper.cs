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

        public string ClrFieldType(FieldType ftype, bool forceNullable = false)
        {
            Func<string, string> Coerce = (clrType) => forceNullable ? clrType += "?" : clrType;

            return ftype switch
            {
                FieldType.DateTime => Coerce("DateTime"),
                FieldType.Float64 => Coerce("double"),
                FieldType.Int64 => Coerce("long"),
                FieldType.String => "string",
                _ => throw new NotImplementedException("FieldType does not have a matching clrType")
            };
        }

        public string ClrDeclString(OperationInfo op)
        {
            string @params = string.Join(", ", op.Arguments.Select(ClrDeclString));
            string returnType = ClrTypeOfReturnValue(op.ReturnType);
            returnType = returnType.Equals("void") ? "Task" : $"Task<{returnType}>";
            return $"{returnType} {op.Name}Async({@params})";
        }

        public string ClrTypeOfReturnValue(OperationInfo.IReturn opReturnInfo)
        {
            if (opReturnInfo is OperationInfo.VoidReturn)
            {
                return "void";
            }
            else if (opReturnInfo is OperationInfo.ScalarReturn sr)
            {
                return ClrFieldType(sr.FieldType);
            }
            else if (opReturnInfo is OperationInfo.EntityReturn er)
            {
                return GetTypeName(er.Entity, GeneratedType.Entity);
            }
            else if (opReturnInfo is OperationInfo.Many many)
            {
                string clrType = ClrTypeOfReturnValue((OperationInfo.IReturn)many.ItemType);
                return $"IEnumerable<{clrType}>";
            }
            else
            {
                throw new NotImplementedException(opReturnInfo.ToString());
            }
        }

        public string ClrDeclString(IArgument arg)
        {
            string typeName;
            if (arg.ArgType is FieldType ftype)
            {
                typeName = ClrFieldType(ftype);
            }
            else if (arg.ArgType is Entity entity)
            {
                // TODO: This is incorrect
                typeName = GetTypeName(entity, GeneratedType.Entity);
            }
            else
            {
                throw new NotImplementedException();
            }

            return $"{typeName} {arg.Name}";
        }

    }
}
