using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    record ClrRecord(
        Core.RecordEntity Source,
        string ClassName,
        IReadOnlyCollection<IClrProperty> Properties
    )
    {
        public override string ToString()
        {
            var props = string.Join("\n", Properties.Select(prop => prop.ToString()));
            return props;
        }

        /// TODO Hacky hack!
        public static Core.Fields.Field GetOutputField(IClrProperty prop)
        {
            if (prop is ClrProperty standard)
            {
                return standard.Source;
            }
            else if (prop is ClrShadowProperty shadow)
            {

                return shadow.Source.Target with { Name = prop.Name, IsRequiredOnCreate = !prop.DefaultsToNull };
            }
            throw new NotImplementedException();
        }
    }

    interface IClrProperty
    {
        ClrRecord Parent { get; }

        string Name { get; }

        // String representation of type e.g. 'int', 'DateTime', 'CustomEntity'
        string ClrType { get; }

        /// e.g. true  -> 'string', 'int?', 'CustomType', List<?>;
        //       false -> 'int', 'DateTime', 'double'
        bool DefaultsToNull { get; }

        /// e.g. true when 'int?', 'DateTime?', 'double?'
        bool IsClrSystemNullable { get; }
    }

    record class ClrProperty(
        ClrRecord Parent,
        Core.Fields.Field Source,
        string Name,
        string ClrType,
        bool DefaultsToNull,
        bool IsClrSystemNullable
    ) : IClrProperty
    {
        public static string ToString(IClrProperty prop)
        {
            string isNullable = prop.IsClrSystemNullable ? "System.Nullable<?>" : "";
            string defaultsToNull = prop.DefaultsToNull ? "def=null" : "def=val";

            return $"{prop.Parent.ClassName}.{prop.Name}: {prop.ClrType}" +
                $"// {defaultsToNull} {isNullable}";
        }

        public override string ToString()
        {
            return ToString(this);
        }
    }

    record class ClrShadowProperty(
        ClrRecord Parent,
        Core.Fields.ShadowField Source,
        string Name,
        string ClrType,
        bool DefaultsToNull,
        bool IsClrSystemNullable
    ) : IClrProperty
    {
        public override string ToString()
        {
            return ClrProperty.ToString(this) + " Shadow";
        }
    }

    // Converts from the internal schema Record Type to the ClrEntity type
    class SchemaToClrConverter
    {
        public readonly ClrTypeMapper typeMapper = new ClrTypeMapper();

        // TODO: Return should be a records and enums
        public IReadOnlyCollection<ClrRecord> Convert(Core.Schema schema)
        {
            List<ClrRecord> clrRecords = new List<ClrRecord>();
            var schemaRecords = schema.Entities
                                .Where(e => e.EntityType == Core.EntityType.Record)
                                .Cast<Core.RecordEntity>();

            foreach (var schemaRecord in schemaRecords)
            {
                clrRecords.Add(Convert(schemaRecord, schema));
            }
            return clrRecords;
        }

        private ClrRecord Convert(RecordEntity schemaRecord, Schema schema)
        {
            string clrRecName = typeMapper.NameOfEntity(schemaRecord);
            var props = new List<IClrProperty>();

            var clrRecord = new ClrRecord(schemaRecord, clrRecName, props);
            foreach (var schemaField in schemaRecord.Fields)
            {
                props.Add(Convert(schemaField, clrRecord, schema));
            }
            foreach (Core.Fields.ShadowField shadow in schemaRecord.ShadowFields())
            {
                props.Add(Convert(shadow, clrRecord, schema));
            }
            return clrRecord;
        }

        private ClrProperty Convert(Core.Fields.Field schemaField, ClrRecord clrRecord, Schema schema)
        {
            var isFieldMandatory = Core.Fields.FieldTypeHelper.IsMandatory(schemaField);
            var typeInfo = typeMapper.ClrNetTypeInfoOf(schemaField.FieldType, isFieldMandatory);
            return new ClrProperty(
                        clrRecord,
                        schemaField,
                        schemaField.Name,
                        typeInfo.AsText,
                        !typeInfo.DefaultsToSomeZero,
                        typeInfo.IsClrSystemNullable);
        }

        private ClrShadowProperty Convert(Core.Fields.ShadowField schemaShadowField, ClrRecord clrRecord, Schema schema)
        {
            var isFieldMandatory = Core.Fields.FieldTypeHelper.IsMandatory(schemaShadowField.Pointer);
            var typeInfo = typeMapper.ClrNetTypeInfoOf(schemaShadowField.Target.FieldType, isFieldMandatory);

            return new ClrShadowProperty(
                        clrRecord,
                        schemaShadowField,
                        schemaShadowField.Name,
                        typeInfo.AsText,
                        !typeInfo.DefaultsToSomeZero,
                        typeInfo.IsClrSystemNullable);
        }


    }

}