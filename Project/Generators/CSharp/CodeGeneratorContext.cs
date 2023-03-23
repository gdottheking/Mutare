using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;
using Sharara.EntityCodeGen.Core.Fields.Types;
using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    internal partial class CodeGeneratorContext
    {

        private readonly ClrTypeMapper typeMapper = new ClrTypeMapper();

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

        public string ClrDeclString(IProcedure proc)
        {
            string @params = string.Join(", ", proc.Arguments.Select(ClrDeclString));
            string returnType = typeMapper.MapToDotNetType(proc.ReturnType);
            returnType = returnType.Equals("void") ? "Task" : $"Task<{returnType}>";

            // HACK: ClrDeclString isn't only called by service Generator
            // Using it here is a hack to make sure a procedure produces the same name
            // In all classes i.e. Service, Repository, etc.
            var svcProc = new ServiceProcClr(proc);
            return $"{returnType} {svcProc.MethodName}({@params})";
        }

        public string ClrDeclString(Argument arg)
        {
            string typeName = typeMapper.MapToDotNetType(arg.Type);
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

    class ClrTypeMapper
    {

        public string NameOfEntity(RecordEntity record)
        {
            return record.Name + "Entity";
        }

        public string NameOfEnum(EnumEntity enumEntity)
        {
            return enumEntity.Name + "Enum";
        }

        [Obsolete]
        public string MapToDotNetType(FieldType fieldType, bool forceNullable = false)
        {
            TypeInfo typeInfo = ClrNetTypeInfoOf(fieldType);
            if (typeInfo.DefaultsToSomeZero && forceNullable)
            {
                return typeInfo.AsText + "?";
            }
            return typeInfo.AsText;
        }

        public TypeInfo ClrNetTypeInfoOf(FieldType fieldType)
        {
            Func<Entity, TypeInfo> MapEntity = entity =>
            {
                if (entity.EntityType == EntityType.Record)
                {
                    return new(fieldType, NameOfEntity((RecordEntity)entity), false, false, false);
                }
                else if (entity.EntityType == EntityType.Enum)
                {
                    return new(fieldType, NameOfEnum((EnumEntity)entity), true, false, false);
                }
                throw new InvalidOperationException("Unexpected entity type");
            };

            return fieldType switch
            {
                FieldType.Void x => new(fieldType, x.ClrType, false, false, false),
                FieldType.DateTime x => new(fieldType, x.ClrType, true, false, false),
                FieldType.Float64 x => new(fieldType, x.ClrType, true, false, false),
                FieldType.Int64 x => new(fieldType, x.ClrType, true, false, false),
                FieldType.Int32 x => new(fieldType, x.ClrType, true, false, false),
                FieldType.String x => new(fieldType, x.ClrType, false, false, false),
                FieldType.List x => new(fieldType, $"List<{ClrNetTypeInfoOf(x.ItemType).AsText}>", false, false, true),
                FieldType.Entity x => MapEntity(x.GetEntity()),
                _ => throw new NotImplementedException($"FieldType {fieldType} does not have a matching clrType")
            };
        }

        public record TypeInfo(FieldType FieldType, string AsText, bool DefaultsToSomeZero, bool IsClrSystemNullable, bool IsList);

        public TypeInfo ClrNetTypeInfoOf(FieldType fieldType, bool isMandatory)
        {
            TypeInfo typeInfo = ClrNetTypeInfoOf(fieldType);
            if (typeInfo.DefaultsToSomeZero && !isMandatory)
            {
                // When a property is not mandatory,
                // and of type int, long, float, double, DateTime
                // We convert the field to its System.Nullable<?> version
                return new TypeInfo(fieldType, typeInfo.AsText + "?", false, true, false);
            }

            return typeInfo;

        }

        [Obsolete]
        public string ClrTypeOf(Field field)
        {
            return ClrNetTypeInfoOf(field.FieldType, field.IsMandatory()).AsText;
        }
    }
}
