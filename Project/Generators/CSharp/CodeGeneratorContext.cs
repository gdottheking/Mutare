using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;
using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal partial class CodeGeneratorContext
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

        public string MapToDotNetType(FieldType fieldType, bool forceNullable = false)
        {
            Func<string, string> Coerce = (clrType) => forceNullable ? clrType += "?" : clrType;

            return fieldType switch
            {
                FieldType.Void x => x.ClrType,
                FieldType.DateTime x => Coerce(x.ClrType),
                FieldType.Float64 x => Coerce(x.ClrType),
                FieldType.Int64 x => Coerce(x.ClrType),
                FieldType.Int32 x => Coerce(x.ClrType),
                FieldType.String x => Coerce(x.ClrType),
                FieldType.List l => $"IEnumerable<{MapToDotNetType(l.ItemType)}>",
                FieldType.Entity x => GetTypeName(x.GetEntity(), GeneratedType.Entity),
                _ => throw new NotImplementedException($"FieldType {fieldType} does not have a matching clrType")
            };
        }

        public string MapToDotNetType(Field field)
        {
            // When a property is not mandatory,
            // and of type int, long, float, double, DateTime
            // We convert the field to its Nullable<?> version
            var clrType = MapToDotNetType(field.FieldType);
            if ((field.FieldType.IsNumericPrimitive() ||
                 field.FieldType is FieldType.DateTime) &&
                 !field.IsMandatory())
            {
                clrType += "?";
            }
            return clrType;
        }

        public string ClrDeclString(IProcedure proc)
        {
            string @params = string.Join(", ", proc.Arguments.Select(ClrDeclString));
            string returnType = MapToDotNetType(proc.ReturnType);
            returnType = returnType.Equals("void") ? "Task" : $"Task<{returnType}>";

            // HACK: ClrDeclString isn't only called by service Generator
            // Using it here is a hack to make sure a procedure produces the same name
            // In all classes i.e. Service, Repository, etc.
            var svcProc = new ServiceProcClr(proc);
            return $"{returnType} {svcProc.MethodName}({@params})";
        }

        public string ClrDeclString(Argument arg)
        {
            string typeName = MapToDotNetType(arg.Type);
            return $"{typeName} {arg.Name}";
        }

        public Entity GetEntity(FieldType fieldType)
        {
            if (fieldType is FieldType.Entity entf)
            {
                return entf.GetEntity();
            }
            else if (fieldType is FieldType.List ftl)
            {
                return GetEntity(ftl.ItemType);
            }
            throw new InvalidOperationException();

        }
    }
}
