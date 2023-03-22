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

        public string RepositoryInterfaceName => "IRepository";
        public string RepositoryClassName => "Repository";
        public string ServiceClassName => "Service";
        public string OutputFolder { get; }

        public CodeWriter GetWriter(RecordEntity record, RecordFile fileType)
        {
            var className = MapToDotNetType(record, fileType);
            return GetWriter(className);
        }

        public CodeWriter GetWriter(EnumEntity enumEntity, EnumFile fileType)
        {
            var className = MapToDotNetType(enumEntity, fileType);
            return GetWriter(className);
        }

        public CodeWriter GetWriter(string className)
        {
            var fileName = $"{OutputFolder}/{className}.cs";
            var writer = File.CreateText(fileName);
            return new CodeWriter(writer);
        }

        public string MapToDotNetType(RecordEntity record, RecordFile recordFile = RecordFile.Entity)
        {
            ArgumentNullException.ThrowIfNull(record.Name);
            return recordFile switch
            {
                RecordFile.Converter => record.Name + "Converter",
                RecordFile.Entity => record.Name + "Entity",
                _ => throw new InvalidOperationException()
            };
        }

        public string MapToDotNetType(EnumEntity enumEntity, EnumFile enumFile = EnumFile.Enum)
        {
            ArgumentNullException.ThrowIfNull(enumEntity.Name);
            return enumFile switch
            {
                EnumFile.Enum => enumEntity.Name + "Enum",
                EnumFile.Entity => enumEntity.Name + "Entity",
                EnumFile.Converter => enumEntity.Name + "Converter",
                _ => throw new InvalidOperationException()
            };
        }

        public string MapToDotNetType(FieldType fieldType, bool forceNullable = false)
        {
            Func<string, string> Coerce = (clrType) => forceNullable ? clrType += "?" : clrType;

            Func<Entity, string> MapEntity =  entity =>
            {
                if (entity.EntityType == EntityType.Record)
                {
                    return MapToDotNetType((RecordEntity)entity);
                }
                else if (entity.EntityType == EntityType.Enum)
                {
                    return MapToDotNetType((EnumEntity)entity);
                }
                throw new InvalidOperationException("Unexpected entity type");
            };

            return fieldType switch
            {
                FieldType.Void x => x.ClrType,
                FieldType.DateTime x => Coerce(x.ClrType),
                FieldType.Float64 x => Coerce(x.ClrType),
                FieldType.Int64 x => Coerce(x.ClrType),
                FieldType.Int32 x => Coerce(x.ClrType),
                FieldType.String x => Coerce(x.ClrType),
                FieldType.List x => $"IEnumerable<{MapToDotNetType(x.ItemType)}>",
                FieldType.Entity x => MapEntity(x.GetEntity()),
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
